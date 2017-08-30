using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IShoppingListRepository : IRepository<ShoppingList>
    {
        Task<ShoppingList> GetOpen(string loggedInUserToken);
        Task<List<ShoppingList>> GetClosed(string loggedInUserToken, int page, int pageSize);
    }
}
