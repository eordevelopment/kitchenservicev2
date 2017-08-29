using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class ShoppingListRepository : Repository<ShoppingList>, IShoppingListRepository
    {
        public ShoppingListRepository(IDbContext context) : base(context, "shoppingLists")
        {
        }

        public Task<ShoppingList> GetOpen(string loggedInUserToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
