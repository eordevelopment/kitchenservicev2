using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IDbContext
    {
        IMongoDatabase Db { get; }
    }
}
