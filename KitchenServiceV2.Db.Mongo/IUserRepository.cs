using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> FindByGoogleId(string sub);
    }
}
