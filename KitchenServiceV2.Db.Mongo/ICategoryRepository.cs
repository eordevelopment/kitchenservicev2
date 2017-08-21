using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category> Find(string userToken, string name);
    }
}
