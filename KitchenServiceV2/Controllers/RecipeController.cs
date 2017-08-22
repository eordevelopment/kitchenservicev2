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

            var objectId = new ObjectId(id);
            var recipe = await this._recipeRepository.Get(objectId);
            if (recipe == null) throw new ArgumentException($"No resource with id: {id}");

            var dto = Mapper.Map<RecipeDto>(recipe);

            var type = await _recipeTypeRepository.Get(recipe.RecipeTypeId);
            if (type != null) dto.RecipeType = Mapper.Map<RecipeTypeDto>(type);

            var itemIds = recipe.RecipeItems.Select(x => x.ItemId).Distinct().ToList();
            var itemsById = (await this._itemRepository.Get(itemIds)).ToDictionary(x => x.Id.ToString());

            foreach (var recipeItem in dto.RecipeItems)
            {
                var item = itemsById.ContainsKey(recipeItem.ItemId) ? itemsById[recipeItem.ItemId] : null;
                if(item == null) continue;

                recipeItem.Name = item.Name;
                recipeItem.Quantity = item.Quantity;
                recipeItem.UnitType = item.UnitType;
            }

            return dto;
        }
    }
}
