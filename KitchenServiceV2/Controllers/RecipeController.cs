﻿using System;
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
    public class RecipeController : BaseController
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IRecipeTypeRepository _recipeTypeRepository;
        private readonly IItemRepository _itemRepository;

        public RecipeController(IRecipeRepository recipeRepository, IRecipeTypeRepository recipeTypeRepository, IItemRepository itemRepository)
        {
            this._recipeRepository = recipeRepository;
            this._recipeTypeRepository = recipeTypeRepository;
            this._itemRepository = itemRepository;
        }

        [HttpGet]
        public async Task<List<RecipeDto>> Get()
        {
            var recipes = await this._recipeRepository.GetAll(this.LoggedInUserToken);
            var result = new List<RecipeDto>();
            if (recipes == null) return result;

            var recipeTypeIds = recipes.Select(x => x.RecipeTypeId).Distinct().ToList();

            var recipeTypesById = (await this._recipeTypeRepository.Get(recipeTypeIds)).ToDictionary(x => x.Id, Mapper.Map<RecipeTypeDto>);

            foreach (var recipe in recipes)
            {
                var dto = Mapper.Map<RecipeDto>(recipe);
                dto.RecipeType = recipeTypesById.ContainsKey(recipe.RecipeTypeId) ? recipeTypesById[recipe.RecipeTypeId] : null;

                result.Add(dto);
            }
            return result;
        }

        [HttpGet("{id}")]
        public async Task<RecipeDto> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var recipe = await this._recipeRepository.Get(objectId);
            if (recipe == null) throw new ArgumentException($"No resource with id: {id}");

            var dto = Mapper.Map<RecipeDto>(recipe);

            var type = await _recipeTypeRepository.Get(recipe.RecipeTypeId);
            if (type != null) dto.RecipeType = Mapper.Map<RecipeTypeDto>(type);

            var itemIds = recipe.RecipeItems.Select(x => x.ItemId).Distinct().ToList();
            var itemsById = (await this._itemRepository.Get(itemIds)).ToDictionary(x => x.Id.ToString());

            foreach (var recipeItem in dto.RecipeItems)
            {
                var item = itemsById.ContainsKey(recipeItem.Item.Id) ? itemsById[recipeItem.Item.Id] : null;
                if(item == null) continue;

                recipeItem.Item.Name = item.Name;
                recipeItem.Item.Quantity = item.Quantity;
                recipeItem.Item.UnitType = item.UnitType;
            }

            return dto;
        }

        [HttpPost]
        public async Task Post([FromBody] RecipeDto value)
        {
            this.ValidateRecipe(value);

            var existingRecipe = await this._recipeRepository.Find(LoggedInUserToken, value.Name);
            if (existingRecipe != null)
            {
                throw new InvalidOperationException("Recipe already exists.");
            }

            var recipe = Mapper.Map<Recipe>(value);
            recipe.UserToken = LoggedInUserToken;
            recipe.Key = GuidEncoder.Encode(Guid.NewGuid());
            recipe.RecipeTypeId = new ObjectId(value.RecipeType.Id);

            if (recipe.RecipeItems != null && recipe.RecipeItems.Any())
            {
                await this.PopulateRecipeItems(value, recipe);
            }

            await this._recipeRepository.Upsert(recipe);
        }

        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] RecipeDto value)
        {
            this.ValidateRecipe(value);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Please provide an id");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var existingRecipe = await this._recipeRepository.Get(objectId);
            if (existingRecipe == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            var recipe = Mapper.Map<Recipe>(value);
            recipe.UserToken = LoggedInUserToken;
            recipe.Id = objectId;
            recipe.RecipeTypeId = new ObjectId(value.RecipeType.Id);

            if (recipe.RecipeItems != null && recipe.RecipeItems.Any())
            {
                await this.PopulateRecipeItems(value, recipe);
            }

            await this._recipeRepository.Upsert(recipe);
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var recipe = await this._recipeRepository.Get(objectId);
            if (recipe == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }
            await this._recipeRepository.Remove(objectId);
        }

        [NonAction]
        private void ValidateRecipe(RecipeDto value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidOperationException("Name cannot be empty");
            }
            if (value.RecipeItems != null && value.RecipeItems.Any(x => string.IsNullOrEmpty(x.Item.Name)))
            {
                throw new InvalidOperationException("Item name cannot be empty");
            }
            if (value.RecipeSteps != null && value.RecipeSteps.Any(x => string.IsNullOrWhiteSpace(x.Description)))
            {
                throw new InvalidOperationException("Description cannot be empty");
            }
        }

        [NonAction]
        private async Task PopulateRecipeItems(RecipeDto value, Recipe recipe)
        {
            var items = value.RecipeItems.Where(x => x.Item != null)
                .GroupBy(x => x.Item.Name)
                .Select(x => x.First())
                .Select(x =>
                {
                    var itm = Mapper.Map<Item>(x.Item);
                    itm.UserToken = LoggedInUserToken;
                    return itm;
                })
                .ToList();
            await this._itemRepository.Upsert(items);
            var itemsByName = items.ToDictionary(x => x.Name);

            recipe.RecipeItems.Clear();
            foreach (var dto in value.RecipeItems)
            {
                var recipeItem = Mapper.Map<RecipeItem>(dto);
                recipeItem.ItemId = itemsByName[dto.Item.Name.ToLower()].Id;
                recipe.RecipeItems.Add(recipeItem);
            }
        }
    }
}
