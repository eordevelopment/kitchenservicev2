using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class ShoppingListRepository : Repository<ShoppingList>, IShoppingListRepository
    {
        public ShoppingListRepository(IDbContext context) : base(context, "shoppingLists")
        {
        }

        public Task<ShoppingList> GetOpen(string loggedInUserToken)
        {
            return this.Collection
                .Find(x => x.UserToken == loggedInUserToken && !x.IsDone)
                .FirstOrDefaultAsync();
        }

        public Task<List<ShoppingList>> GetClosed(string loggedInUserToken, int page, int pageSize)
        {
            return this.Collection
                .Find(p => p.UserToken == loggedInUserToken && p.IsDone)
                .SortByDescending(x => x.CreatedOnUnixSeconds)
                .Skip(page * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
    }
}
