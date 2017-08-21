using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(IDbContext context) : base(context, "accounts")
        {
        }

        public Task<Account> FindByToken(string token)
        {
            return this.Collection.Find(x => x.UserToken == token).FirstOrDefaultAsync();
        }

        public Task<Account> GetUser(string userName)
        {
            return this.Collection.Find(x => x.UserName == userName.ToLower()).FirstOrDefaultAsync();
        }

        public Task<Account> GetUser(string userName, string hashedPassword)
        {
            return this.Collection.Find(x => x.UserName == userName.ToLower() && x.HashedPassword == hashedPassword).FirstOrDefaultAsync();
        }
    }
}
