using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(IDbContext context) : base(context, "recipes")
        {
        }

        public Task<Recipe> Find(string userToken, string name)
        {
            return this.Collection.Find(x => x.UserToken == userToken && x.Name == name.ToLower()).FirstOrDefaultAsync();
        }

        public Task<Recipe> Find(string key)
        {
            return this.Collection.Find(x => x.Key == key).FirstOrDefaultAsync();
        }

        public Task<List<Recipe>> FindByItem(string userToken, ObjectId itemId)
        {
            return this.Collection.Find(x => x.UserToken == userToken && x.RecipeItems.Any(item => item.ItemId == itemId)).ToListAsync();
        }
    }
}
