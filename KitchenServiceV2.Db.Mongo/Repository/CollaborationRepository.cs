using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class CollaborationRepository : Repository<Collaboration>, ICollaborationRepository
    {
        public CollaborationRepository(IDbContext context) : base(context, "collaboration")
        {
        }

        public Task<List<Collaboration>> FindPending(string pendingUserEmailAddress)
        {
            return this.Collection
                .Find(p => p.Collaborators.Any(x => (x.UserId == null || x.UserId == ObjectId.Empty) && x.Email == pendingUserEmailAddress))
                .ToListAsync();
        }
    }
}
