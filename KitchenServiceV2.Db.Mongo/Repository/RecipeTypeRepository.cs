using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class RecipeTypeRepository : Repository<RecipeType>, IRecipeTypeRepository
    {
        public RecipeTypeRepository(IDbContext context) : base(context, "recipeTypes")
        {
        }

        public Task<RecipeType> Find(string userToken, string name)
        {
            return this.Collection.Find(x => x.UserToken == userToken && x.Name == name.ToLower()).FirstOrDefaultAsync();
        }
    }
}
