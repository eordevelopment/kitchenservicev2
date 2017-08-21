using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IItemRepository : IRepository<Item>
    {
        Task<Item> FindItem(string userToken, string name);
        Task<List<Item>> SearchItems(string userToken, string name, int max);
    }
}
