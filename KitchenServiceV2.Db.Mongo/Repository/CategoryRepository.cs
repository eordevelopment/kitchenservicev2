using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(IDbContext context) : base(context, "categories")
        {
        }

        public Task<Category> Find(string userToken, string name)
        {
            return this.Collection.Find(x => x.UserToken == userToken && x.Name == name.ToLower()).FirstOrDefaultAsync();
        }
    }
}
