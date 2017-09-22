using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDbContext context) : base(context, "users")
        {
        }

        public Task<User> FindByGoogleId(string sub)
        {
            return this.Collection.Find(x => x.Sub == sub).FirstOrDefaultAsync();
        }
    }
}
