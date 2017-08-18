using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IRepository<TEntity> where TEntity : IDocument
    {
        Task<IEnumerable<TEntity>> GetAll();
        Task<TEntity> Get(ObjectId id);
        Task<List<TEntity>> Get(IReadOnlyCollection<ObjectId> ids);
        Task Insert(TEntity p);
        Task Update(TEntity entity);
        Task Remove(TEntity entity);
        Task Remove(ObjectId id);
    }
}
