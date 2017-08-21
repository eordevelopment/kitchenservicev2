using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IRecipeTypeRepository : IRepository<RecipeType>
    {
        Task<RecipeType> Find(string userToken, string name);
    }
}
