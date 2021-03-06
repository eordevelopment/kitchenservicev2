﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class ItemToBuyRepository : Repository<ItemToBuy>, IItemToBuyRepository
    {
        public ItemToBuyRepository(IDbContext context) : base(context, "itemstobuy")
        {
        }

        public Task<ItemToBuy> FindByItemId(ObjectId itemId)
        {
            return this.Collection.Find(x => x.ItemId == itemId).FirstOrDefaultAsync();
        }

        public Task Remove(string userToken)
        {
            return this.Collection.DeleteManyAsync(i => i.UserToken == userToken);
        }

        public Task<List<ItemToBuy>> FindByItemIds(string userToken, IReadOnlyCollection<ObjectId> itemIds)
        {
            if (itemIds == null || !itemIds.Any()) return Task.FromResult(new List<ItemToBuy>());
            return this.Collection.Find(p => p.UserToken == userToken && itemIds.Contains(p.ItemId)).ToListAsync();
        }
    }
}
