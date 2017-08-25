using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class PlanRepository : Repository<Plan>, IPlanRepository
    {
        public PlanRepository(IDbContext context) : base(context, "plans")
        {
        }

        public Task<List<Plan>> GetOpen(string userToken, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public Task<List<Plan>> GetClosed(string userToken, int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
