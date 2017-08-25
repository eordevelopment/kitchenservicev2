using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IPlanRepository : IRepository<Plan>
    {
        Task<List<Plan>> GetOpen(string userToken, DateTime start, DateTime end);
        Task<List<Plan>> GetClosed(string userToken, int page, int pageSize);
    }
}
