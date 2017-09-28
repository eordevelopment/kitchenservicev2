using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        public ItemRepository(IDbContext context) : base(context, "items")
        {
        }

        public Task<Item> FindItem(string userToken, string name)
        {
            return this.Collection.Find(p => p.UserToken == userToken && p.Name == name.ToLower()).FirstOrDefaultAsync();
        }

        public Task<List<Item>> SearchItems(string userToken, string name, int max)
        {
            return this.Collection
                .Find(p => p.UserToken == userToken && p.Name.StartsWith(name.ToLower()))
                .Limit(max)
                .ToListAsync();
        }

        public Task<long> CountItems(string userToken, string name)
        {
            return this.Collection.CountAsync(p => p.UserToken == userToken && p.Name.StartsWith(name.ToLower()));
        }
    }
}
