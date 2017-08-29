using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "HasToken")]
    public class ListController : BaseInventoryController
    {
        private readonly IShoppingListRepository _shoppingListRepository;

        public ListController(
            IPlanRepository planRepository, 
            IItemRepository itemRepository, 
            IShoppingListRepository shoppingListRepository,
            IRecipeRepository recipeRepository) : base(planRepository, itemRepository, recipeRepository)
        {
            this._shoppingListRepository = shoppingListRepository;
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
            if (list == null) throw new ArgumentException($"No resource with id: {id}");

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

            var itemsById = (await this.GetRecipeItems(recipes)).ToDictionary(x => x.Id);
            if (!itemsById.Any()) return string.Empty;

            var recipItemsById = recipes
                .SelectMany(x => x.RecipeItems)
                .ToLookup(x => x.ItemId);

            var now = DateTimeOffset.UtcNow;
            var shoppingList = new ShoppingList
            {
                CreatedOnUnixSeconds = now.ToUnixTimeSeconds(),
                Name = now.ToString("ddd, MMM-dd yyyy"),
                IsDone = false,
                UserToken = LoggedInUserToken,
                Items = new List<ShoppingListItem>(),
                OptionalItems = new List<ShoppingListItem>()
            };


            foreach (var itemCollection in recipItemsById)
            {
                if (!itemsById.ContainsKey(itemCollection.Key)) continue;

                float inStock = 0, needed = 0;
                var item = itemsById[itemCollection.Key];

                foreach (var recipeItem in itemCollection)
                {
                    inStock = item.Quantity;
                    needed += recipeItem.Amount;
                }

                var shoppingListItem = new ShoppingListItem
                {
                    ItemId = itemCollection.Key,
                    IsDone = false,
                    Amount = needed - inStock,
                    TotalAmount = needed
                };

                if (needed > inStock)
                {
                    shoppingList.Items.Add(shoppingListItem);
                }
                else
                {
                    shoppingListItem.Amount = shoppingListItem.TotalAmount;
                    shoppingList.OptionalItems.Add(shoppingListItem);
                }
            }

            await this._shoppingListRepository.Upsert(shoppingList);
            return shoppingList.Id.ToString();
        }

        //[HttpGet("/api/list/closed/{page}")]
        //public IEnumerable<ShoppingListDto> GetClosedLists(int page = 0)
        //{
            
        //}

        //[HttpPut("{id}")]
        //public ShoppingListDto Put(int id, [FromBody] ShoppingListDto value)
        //{
            
        //}

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var shoppingList = await this._shoppingListRepository.Get(objectId);
            if (shoppingList == null)
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

            var itemsById = (await this.ItemRepository.Get(itemIds)).ToDictionary(x => x.Id.ToString());
            SetItems(dto.Items, itemsById);
            SetItems(dto.OptionalItems, itemsById);
        }

        [NonAction]
        private static void SetItems(IEnumerable<ShoppingListItemDto> items, IReadOnlyDictionary<string, Item> itemsById)
        {
            foreach (var itemDto in items)
            {
                if (!itemsById.ContainsKey(itemDto.Item.Id)) continue;
                itemDto.Item = Mapper.Map<ItemDto>(itemsById[itemDto.Item.Id]);
            }
        }
    }
}
