using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class PlanRepository : Repository<Plan>, IPlanRepository
    {
        public PlanRepository(IDbContext context) : base(context, "plans")
        {
        }

        public Task<List<Plan>> GetOpenOrInRange(string userToken, DateTimeOffset start, DateTimeOffset end)
        {
            return this.Collection
                .Find(p => p.UserToken == userToken && (!p.IsDone || (p.DateTimeUnixSeconds >= start.ToUnixTimeSeconds() && p.DateTimeUnixSeconds <= end.ToUnixTimeSeconds())))
                .ToListAsync();
        }

        public Task<List<Plan>> GetClosed(string userToken, int page, int pageSize)
        {
            return this.Collection
                .Find(p => p.UserToken == userToken && p.IsDone)
                .SortByDescending(x => x.DateTimeUnixSeconds)
                .Skip(page * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public Task<Plan> Find(string userToken, DateTimeOffset dateTime)
        {
            return this.Collection
                .Find(x => x.UserToken == userToken && x.DateTimeUnixSeconds == dateTime.ToUnixTimeSeconds())
                .FirstOrDefaultAsync();
        }

        public Task<List<Plan>> GetOpen(string userToken)
        {
            return this.Collection
                .Find(p => p.UserToken == userToken && !p.IsDone)
                .ToListAsync();
        }

        public Task<List<Plan>> GetRecipePlans(ObjectId recipeId)
        {
            return this.Collection
                .Find(p => !p.IsDone && p.PlanItems.Any(x => !x.IsDone && x.RecipeId == recipeId))
                .SortByDescending(x => x.DateTimeUnixSeconds)
                .ToListAsync();
        }
    }
}
