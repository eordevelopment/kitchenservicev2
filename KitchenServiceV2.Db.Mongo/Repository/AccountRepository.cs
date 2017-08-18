using KitchenServiceV2.Db.Mongo.Schema;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class AccountRepository : Repository<Account>
    {
        public AccountRepository(IDbContext context) : base(context, "passengers")
        {
        }
    }
}
