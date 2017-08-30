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
        private readonly IShoppingListModel _shoppingListModel;

        public ListController(
            IPlanRepository planRepository, 
            IItemRepository itemRepository, 
            IShoppingListRepository shoppingListRepository,
            IRecipeRepository recipeRepository,
            IShoppingListModel shoppingListModel) : base(planRepository, itemRepository, recipeRepository)
        {
            this._shoppingListRepository = shoppingListRepository;
            this._shoppingListModel = shoppingListModel;
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

            var shoppingList = this._shoppingListModel.CreateShoppingList(LoggedInUserToken, recipes, itemsById);

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
