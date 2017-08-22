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
    public class RecipeTypeController : BaseController
    {
        private readonly IRecipeTypeRepository _recipeTypeRepository;

        public RecipeTypeController(IRecipeTypeRepository recipeTypeRepository)
        {
            this._recipeTypeRepository = recipeTypeRepository;
        }

        [HttpGet]
        public async Task<List<RecipeTypeDto>> Get()
        {
            var recipeTypes = await this._recipeTypeRepository.GetAll(this.LoggedInUserToken);
            return recipeTypes == null ? new List<RecipeTypeDto>() : recipeTypes.Select(Mapper.Map<RecipeTypeDto>).ToList();
        }

        [HttpPost]
        public async Task<string> Post([FromBody] RecipeTypeDto value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidOperationException("Name cannot be empty");
            }

            var existing = await this._recipeTypeRepository.Find(LoggedInUserToken, value.Name);
            if (existing != null)
            {
                throw new InvalidOperationException("Recipe Type already exists.");
            }

            var recipeType = Mapper.Map<RecipeType>(value);
            await this._recipeTypeRepository.Insert(recipeType);

            return recipeType.Id.ToString();
        }

        [HttpPut("{id}")]
        public async Task<string> Put(string id, [FromBody] RecipeTypeDto value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidOperationException("Name cannot be empty");
            }

            var recipeTypeId = new ObjectId(id);
            var existing = await this._recipeTypeRepository.Get(recipeTypeId);
            if (existing == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            var recipeType = Mapper.Map<RecipeType>(value);
            recipeType.Id = recipeTypeId;
            recipeType.UserToken = LoggedInUserToken;

            await this._recipeTypeRepository.Update(recipeType);

            return recipeType.Id.ToString();
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var recipeTypeId = new ObjectId(id);
            var existing = await this._recipeTypeRepository.Get(recipeTypeId);
            if (existing == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            await this._recipeTypeRepository.Remove(recipeTypeId);
        }
    }
}
