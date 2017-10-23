using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
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
                .Find(p => p.Collaborators.Any(x => String.IsNullOrEmpty(x.UserToken) && x.Email == pendingUserEmailAddress))
                .ToListAsync();
        }

        public Task<List<Collaboration>> Find(string sharedUserToken)
        {
            return this.Collection
                .Find(p => p.Collaborators.Any(x => x.UserToken.Equals(sharedUserToken)))
                .ToListAsync();
        }
    }
}
