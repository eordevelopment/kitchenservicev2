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
    public class ItemsController : BaseController
    {
        private readonly IItemRepository _itemRepository;
        private readonly IItemToBuyRepository _itemToBuyRepository;
        private readonly IRecipeRepository _recipeRepository;

        public ItemsController(IItemRepository itemRepository, IItemToBuyRepository itemToBuyRepository, IRecipeRepository recipeRepository)
        {
            this._itemRepository = itemRepository;
            this._itemToBuyRepository = itemToBuyRepository;
            this._recipeRepository = recipeRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<ItemDto>> Get()
        {
            var items = await this._itemRepository.GetAll(LoggedInUserToken);
            return (await this.ToContract(items)).OrderBy(x => x.Name);
        }

        [HttpGet("{id}")]
        public async Task<ItemDto> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            var item = await this._itemRepository.Get(objectId);
            if (item == null || item.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            var mustBuyItem = await this._itemToBuyRepository.FindByItemId(objectId);

            var dto = Mapper.Map<ItemDto>(item);
            dto.FlaggedForNextShop = mustBuyItem != null;

            var itemRecipes = await this._recipeRepository.FindByItem(LoggedInUserToken, objectId);
            dto.Recipes = itemRecipes.Select(Mapper.Map<RecipeDto>);

            return dto;
        }

        [HttpPost]
        public async Task<string> Post([FromBody] ItemDto value)
        {
            ValidateItem(value);

            var existingItem = await this._itemRepository.FindItem(LoggedInUserToken, value.Name.ToLower());
            if (existingItem != null)
            {
                throw new InvalidOperationException("Item already exists.");
            }

            var item = Mapper.Map<Item>(value);
            item.UserToken = LoggedInUserToken;

            await this._itemRepository.Upsert(item);
            await this.FlagItemToBuy(item.Id, value.FlaggedForNextShop);
            return item.Id.ToString();
        }

        [HttpPut("{id}")]
        public async Task<string> Put(string id, [FromBody] ItemDto value)
        {
            ValidateItem(value);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            var item = await this._itemRepository.Get(objectId);
            if (item == null || item.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            item = Mapper.Map<Item>(value);
            item.Id = objectId;
            item.UserToken = LoggedInUserToken;

            await this._itemRepository.Upsert(item);
            await this.FlagItemToBuy(item.Id, value.FlaggedForNextShop);
            return item.Id.ToString();
        }

        [HttpGet("/api/items/search/{value}/{pageSize}/{page}")]
        public async Task<ItemSearchResultDto> Search(string value, int pageSize = 10, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new ItemSearchResultDto
                {
                    PageSize = pageSize,
                    TotalResults = 0,
                    Items = Enumerable.Empty<ItemDto>()
                };
            }
            var items = await this._itemRepository.SearchItems(LoggedInUserToken, value, 10);
            var count = await this._itemRepository.CountItems(LoggedInUserToken, value);
            var itemDtos = await this.ToContract(items);
            return new ItemSearchResultDto
            {
                PageSize = pageSize,
                TotalResults = count,
                Items = itemDtos
            };
        }

        [HttpPut("/api/items/flag/{id}")]
        public async Task FlagForShopping(string id, [FromBody]bool mustBuy)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            var item = await this._itemRepository.Get(objectId);
            if (item == null || item.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            await FlagItemToBuy(objectId, mustBuy);
        }

        [NonAction]
        private async Task<IEnumerable<ItemDto>> ToContract(IReadOnlyCollection<Item> items)
        {
            var itemIds = items.Select(x => x.Id).ToList();
            var itemsToBuyByItemId = (await this._itemToBuyRepository.FindByItemIds(LoggedInUserToken, itemIds)).ToDictionary(x => x.Id);

            var results = new List<ItemDto>();
            foreach (var item in items)
            {
                var dto = Mapper.Map<ItemDto>(item);
                dto.FlaggedForNextShop = itemsToBuyByItemId.ContainsKey(item.Id);
                results.Add(dto);
            }
            return results;
        }

        [NonAction]
        private static void ValidateItem(ItemDto value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidOperationException("Name cannot be empty");
            }
        }

        [NonAction]
        private async Task FlagItemToBuy(ObjectId objectId, bool mustBuy)
        {
            var itemToBuy = await this._itemToBuyRepository.FindByItemId(objectId);
            if (itemToBuy == null && mustBuy)
            {
                itemToBuy = new ItemToBuy
                {
                    ItemId = objectId,
                    UserToken = LoggedInUserToken
                };
                await this._itemToBuyRepository.Upsert(itemToBuy);
            }
            if (itemToBuy != null && !mustBuy)
            {
                await this._itemToBuyRepository.Remove(itemToBuy);
            }
        }
    }
}
