using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "HasToken")]
    public class CategoryController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IItemRepository _itemRepository;

        public CategoryController(ICategoryRepository categoryRepository, IItemRepository itemRepository)
        {
            this._categoryRepository = categoryRepository;
            this._itemRepository = itemRepository;
        }

        [HttpGet]
        public async Task<List<CategoryDto>> Get()
        {
            var categories = await this._categoryRepository.GetAll(this.LoggedInUserToken);

            return categories.Select(Mapper.Map<CategoryDto>).ToList();
        }

        [HttpGet("{id}")]
        public async Task<CategoryDto> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = new ObjectId(id);
            var category = await this._categoryRepository.Get(objectId);
            if (category != null)
            {
                var items = await this._itemRepository.Get(category.ItemIds);

                var result = Mapper.Map<CategoryDto>(category);
                result.Items = items.Select(Mapper.Map<ItemDto>).ToList();

                return result;
            }
            throw new ArgumentException($"No resource with id: {id}");
        }
    }
}
