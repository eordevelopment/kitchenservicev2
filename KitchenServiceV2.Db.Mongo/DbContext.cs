using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo
{
    public class DbContext : IDbContext
    {
        public DbContext(string connection, string database)
        {
            this.Db = new MongoClient(connection).GetDatabase(database);
        }

        public IMongoDatabase Db { get; }
    }
}
