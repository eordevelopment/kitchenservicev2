using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface ICollaborationRepository : IRepository<Collaboration>
    {
        Task<List<Collaboration>> FindPending(string pendingUserEmailAddress);
    }
}
