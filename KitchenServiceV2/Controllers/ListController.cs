using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ListController : BaseInventoryController
    {
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly IShoppingListModel _shoppingListModel;
        private readonly IItemToBuyRepository _itemToBuyRepository;

        public ListController(
            IPlanRepository planRepository, 
            IItemRepository itemRepository,
            IItemToBuyRepository itemToBuyRepository,
            IShoppingListRepository shoppingListRepository,
            IRecipeRepository recipeRepository,
            IShoppingListModel shoppingListModel) : base(planRepository, itemRepository, recipeRepository)
        {
            this._shoppingListRepository = shoppingListRepository;
            this._shoppingListModel = shoppingListModel;
            this._itemToBuyRepository = itemToBuyRepository;
        }

        [HttpGet("/api/list/open")]
        public async Task<ShoppingListDto> GetOpen()
        {
            var list = await this._shoppingListRepository.GetOpen(LoggedInUserToken);
            if (list == null) return null;

            var dto = Mapper.Map<ShoppingListDto>(list);
            await this.PopulateItems(dto, list);
           
            return dto;
        }

        [HttpGet("/api/list/details/{id}")]
        public async Task<ShoppingListDto> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var list = await this._shoppingListRepository.Get(objectId);
            if (list == null || list.UserToken != LoggedInUserToken) throw new ArgumentException($"No resource with id: {id}");

            var dto = Mapper.Map<ShoppingListDto>(list);
            await this.PopulateItems(dto, list);

            return dto;
        }

        [HttpGet("/api/list/generate")]
        public async Task<string> GenerateList()
        {
            var list = await this._shoppingListRepository.GetOpen(LoggedInUserToken);
            if (list != null)
            {
                await this._shoppingListRepository.Remove(list.Id);
            }

            var plans = await this.PlanRepository.GetOpen(LoggedInUserToken);
            if (plans == null || !plans.Any()) return string.Empty;

            var recipes = await this.GetPlanRecipes(plans);
            if (!recipes.Any()) return string.Empty;

            var additionalItemsToBuy = await this._itemToBuyRepository.GetAll(LoggedInUserToken);

            var itemsById = (await this.GetItems(recipes, additionalItemsToBuy.Select(x => x.ItemId))).ToDictionary(x => x.Id);
            if (!itemsById.Any()) return string.Empty;

            var recipesById = recipes.ToDictionary(x => x.Id);
            var planRecipes = plans
                .SelectMany(x => x.PlanItems)
                .Select(x => recipesById.ContainsKey(x.RecipeId) ? recipesById[x.RecipeId] : null);

            var shoppingList = this._shoppingListModel.CreateShoppingList(LoggedInUserToken, planRecipes.Where(x => x != null).ToList(), itemsById, additionalItemsToBuy);

            await this._shoppingListRepository.Upsert(shoppingList);
            await this._itemToBuyRepository.Remove(LoggedInUserToken);
            return shoppingList.Id.ToString();
        }

        [HttpGet("/api/list/closed/{page}")]
        public async Task<IEnumerable<ShoppingListDto>> GetClosedLists(int page = 0)
        {
            var lists = await this._shoppingListRepository.GetClosed(LoggedInUserToken, page, 10);
            return lists.Select(Mapper.Map<ShoppingListDto>);
        }

        [HttpPut("{id}")]
        public async Task<ShoppingListDto> Put(string id, [FromBody] ShoppingListDto value)
        {
            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var existingList = await this._shoppingListRepository.Get(objectId);
            if (existingList == null || existingList.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }
            var existingItemsById = existingList.Items.ToDictionary(x => x.ItemId);

            var updatedList = Mapper.Map<ShoppingList>(value);
            updatedList.UserToken = LoggedInUserToken;
            updatedList.Id = existingList.Id;

            var itemIds = updatedList.Items
                .Where(x => x.ItemId != ObjectId.Empty)
                .Select(x => x.ItemId)
                .Concat(updatedList.OptionalItems.Where(x => x.ItemId != ObjectId.Empty).Select(x => x.ItemId))
                .ToList();

            var itemsById = new Dictionary<ObjectId, Item>();
            if (itemIds.Any())
            {
                itemsById = (await this.ItemRepository.Get(itemIds)).ToDictionary(x => x.Id);

                foreach (var shoppingListItem in updatedList.Items)
                {
                    if (!itemsById.ContainsKey(shoppingListItem.ItemId)) continue;
                    var item = itemsById[shoppingListItem.ItemId];

                    var existingItem = existingItemsById.ContainsKey(shoppingListItem.ItemId) ? existingItemsById[shoppingListItem.ItemId] : null;
                    if (shoppingListItem.IsDone && (existingItem == null || !existingItem.IsDone))
                    {
                        // increment stock
                        item.Quantity += shoppingListItem.Amount;
                    }
                    else if (!shoppingListItem.IsDone && existingItem != null && existingItem.IsDone)
                    {
                        //decrement stock
                        item.Quantity -= shoppingListItem.Amount;
                    }
                }

                await this.ItemRepository.Upsert(itemsById.Values);
            }

            await this._shoppingListRepository.Upsert(updatedList);

            var result = Mapper.Map<ShoppingListDto>(updatedList);
            SetItems(result.Items, itemsById, value.Items?.ToDictionary(x => x.Item.Id));
            SetItems(result.OptionalItems, itemsById, value.OptionalItems?.ToDictionary(x => x.Item.Id));

            return result;
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var shoppingList = await this._shoppingListRepository.Get(objectId);
            if (shoppingList == null || shoppingList.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }
            await this._shoppingListRepository.Remove(objectId);
        }

        [NonAction]
        private async Task PopulateItems(ShoppingListDto dto, ShoppingList dbShoppingList)
        {
            var itemIds = dbShoppingList.Items.Select(x => x.ItemId)
                .Concat(dbShoppingList.OptionalItems.Select(x => x.ItemId))
                .Distinct()
                .ToList();

            var recipesIds = dbShoppingList.Items.SelectMany(x => x.RecipeIds)
                .Concat(dbShoppingList.OptionalItems.SelectMany(x => x.RecipeIds))
                .Distinct()
                .ToList();

            var itemsById = (await this.ItemRepository.Get(itemIds)).ToDictionary(x => x.Id.ToString());
            var recipesById = (await this.RecipeRepository.Get(recipesIds)).ToDictionary(x => x.Id.ToString());
            SetItems(dto.Items, itemsById, recipesById);
            SetItems(dto.OptionalItems, itemsById, recipesById);
        }

        [NonAction]
        private static void SetItems(IEnumerable<ShoppingListItemDto> items, IReadOnlyDictionary<string, Item> itemsById, Dictionary<string, Recipe> recipesById)
        {
            foreach (var itemDto in items)
            {
                if (!itemsById.ContainsKey(itemDto.Item.Id)) continue;
                itemDto.Item = Mapper.Map<ItemDto>(itemsById[itemDto.Item.Id]);
                var recipeIds = itemDto.Recipes.Select(x => x.Id);
                itemDto.Recipes = recipesById.Where(x => recipeIds.Contains(x.Key)).Select(r => Mapper.Map<RecipeDto>(r.Value)).ToList();
            }
        }

        [NonAction]
        private static void SetItems(IEnumerable<ShoppingListItemDto> items, IReadOnlyDictionary<ObjectId, Item> itemsById, IReadOnlyDictionary<string, ShoppingListItemDto> itemDtosById)
        {
            foreach (var itemDto in items)
            {
                var id = Mapper.Map<ObjectId>(itemDto.Item.Id);
                if (!itemsById.ContainsKey(id)) continue;
                itemDto.Item = Mapper.Map<ItemDto>(itemsById[id]);
                itemDto.Recipes = itemDtosById != null && itemDtosById.ContainsKey(itemDto.Item.Id) ? itemDtosById[itemDto.Item.Id].Recipes : new List<RecipeDto>();
            }
        }
    }
}
