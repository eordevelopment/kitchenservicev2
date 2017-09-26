using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IItemToBuyRepository : IRepository<ItemToBuy>
    {
        Task<ItemToBuy> FindByItemId(ObjectId itemId);
        Task Remove(string userToken);
    }
}
