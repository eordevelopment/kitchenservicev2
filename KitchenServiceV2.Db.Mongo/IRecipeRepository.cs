using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task<Recipe> Find(string userToken, string name);
        Task<Recipe> Find(string key);
    }
}
