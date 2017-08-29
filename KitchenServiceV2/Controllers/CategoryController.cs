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
            return categories == null ? new List<CategoryDto>() : categories.Select(Mapper.Map<CategoryDto>).ToList();
        }

        [HttpGet("{id}")]
        public async Task<CategoryDto> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            if(objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var category = await this._categoryRepository.Get(objectId);
            if (category == null) throw new ArgumentException($"No resource with id: {id}");

            var items = await this._itemRepository.Get(category.ItemIds);

            var result = Mapper.Map<CategoryDto>(category);
            result.Items = items.Select(Mapper.Map<ItemDto>).ToList();

            return result;
        }

        [HttpPost]
        public async Task Post([FromBody] CategoryDto value)
        {
            ValidateCategory(value);

            var existingCategory = await this._categoryRepository.Find(LoggedInUserToken, value.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException("Category already exists.");
            }

            var category = await this.PopulateCategory(value);

            // save the category
            await this._categoryRepository.Upsert(category);
        }

        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] CategoryDto value)
        {
            ValidateCategory(value);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var existingCategory = await this._categoryRepository.Get(objectId);
            if (existingCategory == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            var category = await this.PopulateCategory(value);
            category.Id = objectId;

            // save the category
            await this._categoryRepository.Upsert(category);
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var existingCategory = await this._categoryRepository.Get(objectId);
            if (existingCategory == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }
            await this._categoryRepository.Remove(objectId);
        }

        private async Task<Category> PopulateCategory(CategoryDto value)
        {
            var category = Mapper.Map<Category>(value);
            category.ItemIds = new List<ObjectId>();
            category.UserToken = LoggedInUserToken;

            if (value.Items == null || !value.Items.Any()) return category;

            var items = value.Items.Select(x =>
            {
                var itm = Mapper.Map<Item>(x);
                itm.UserToken = LoggedInUserToken;
                return itm;
            }).ToList();

            await this._itemRepository.Upsert(items);
            category.ItemIds.AddRange(items.Select(x => x.Id));

            return category;
        }

        [NonAction]
        private static void ValidateCategory(CategoryDto value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidOperationException("Name cannot be empty");
            }
            if (value.Items != null && value.Items.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            {
                throw new InvalidOperationException("Item Name cannot be empty");
            }
        }
    }
}
