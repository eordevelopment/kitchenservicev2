using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ItemsController : BaseController
    {
        private readonly IItemRepository _itemRepository;

        public ItemsController(IItemRepository itemRepository)
        {
            this._itemRepository = itemRepository;
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
    }
}
