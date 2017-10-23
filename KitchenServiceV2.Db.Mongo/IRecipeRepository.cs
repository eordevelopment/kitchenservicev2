using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task<List<Recipe>> GetRecipes(IReadOnlyCollection<String> userTokens);
        Task<Recipe> Find(string userToken, string name);
        Task<Recipe> Find(string key);
        Task<List<Recipe>> FindByItem(string userToken, ObjectId itemId);
    }
}
