using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<Account> FindByToken(string token);
        Task<Account> GetUser(string userName);
        Task<Account> GetUser(string userName, string hashedPassword);
    }
}
