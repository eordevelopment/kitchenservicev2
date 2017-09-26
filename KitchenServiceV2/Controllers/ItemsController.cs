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

        public ItemsController(IItemRepository itemRepository, IItemToBuyRepository itemToBuyRepository)
        {
            this._itemRepository = itemRepository;
            this._itemToBuyRepository = itemToBuyRepository;
        }

        [HttpGet("/api/items/search/{value}")]
        public async Task<IEnumerable<ItemDto>> Search(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<ItemDto>();
            }
            return (await this._itemRepository.SearchItems(LoggedInUserToken, value, 10)).Select(Mapper.Map<ItemDto>);
        }

        [HttpPut("/api/items/flag/{id}")]
        public async Task FlagForShopping(string id)
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

            var itemToBuy = await this._itemToBuyRepository.FindByItemId(objectId);
            if (itemToBuy == null)
            {
                itemToBuy = new ItemToBuy
                {
                    ItemId = objectId,
                    UserToken = LoggedInUserToken
                };
                await this._itemToBuyRepository.Upsert(itemToBuy);
            }
        }
    }
}
