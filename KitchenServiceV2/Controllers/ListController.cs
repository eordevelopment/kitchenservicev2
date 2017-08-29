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
    public class ListController : BaseController
    {
        private readonly IPlanRepository _planRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly IRecipeRepository _recipeRepository;

        public ListController(
            IPlanRepository planRepository, 
            IItemRepository itemRepository, 
            IShoppingListRepository shoppingListRepository,
            IRecipeRepository recipeRepository)
        {
            this._itemRepository = itemRepository;
            this._planRepository = planRepository;
            this._shoppingListRepository = shoppingListRepository;
            this._recipeRepository = recipeRepository;
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

        [HttpGet("/api/list/generate")]
        public async Task<string> GenerateList()
        {
            var list = await this._shoppingListRepository.GetOpen(LoggedInUserToken);
            if (list != null)
            {
                await this._shoppingListRepository.Remove(list.Id);
            }

            var plans = await this._planRepository.GetOpen(LoggedInUserToken);

            if (plans != null && plans.Any())
            {
                var recipeIds = plans.SelectMany(x => x.PlanItems).Select(x => x.RecipeId).Distinct().ToList();
                if (recipeIds.Any())
                {
                    var recipes = await this._recipeRepository.Get(recipeIds);
                }
            }

            return string.Empty;
        }

        [NonAction]
        private async Task PopulateItems(ShoppingListDto dto, ShoppingList dbShoppingList)
        {
            var itemIds = dbShoppingList.Items.Select(x => x.ItemId)
                .Concat(dbShoppingList.OptionalItems.Select(x => x.ItemId))
                .Distinct()
                .ToList();

            var itemsById = (await this._itemRepository.Get(itemIds)).ToDictionary(x => x.Id.ToString());
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
    }
}
