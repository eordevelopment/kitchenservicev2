using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IPlanRepository : IRepository<Plan>
    {
        Task<List<Plan>> GetOpenOrInRange(string userToken, DateTimeOffset start, DateTimeOffset end);
        Task<List<Plan>> GetClosed(string userToken, int page, int pageSize);

        Task<Plan> Find(string userToken, DateTimeOffset dateTime);
        Task<List<Plan>> GetOpen(string userToken);
        Task<List<Plan>> GetRecipePlans(ObjectId recipeId);
    }
}
