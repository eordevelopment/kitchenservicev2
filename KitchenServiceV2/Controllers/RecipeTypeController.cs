﻿using System;
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
            recipeType.UserToken = LoggedInUserToken;
            await this._recipeTypeRepository.Upsert(recipeType);

            return recipeType.Id.ToString();
        }

        [HttpPut("{id}")]
        public async Task<string> Put(string id, [FromBody] RecipeTypeDto value)
        {
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                throw new InvalidOperationException("Name cannot be empty");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var existing = await this._recipeTypeRepository.Get(objectId);
            if (existing == null || existing.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            var recipeType = Mapper.Map<RecipeType>(value);
            recipeType.Id = objectId;
            recipeType.UserToken = LoggedInUserToken;

            await this._recipeTypeRepository.Upsert(recipeType);

            return recipeType.Id.ToString();
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var existing = await this._recipeTypeRepository.Get(objectId);
            if (existing == null || existing.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            await this._recipeTypeRepository.Remove(objectId);
        }
    }
}
