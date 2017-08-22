using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo
{
    public interface IRepository<TEntity> where TEntity : IDocument
    {
        string CollectionName { get; }

        Task<List<TEntity>> GetAll(string userToken);
        Task<TEntity> Get(ObjectId id);
        Task<List<TEntity>> Get(IReadOnlyCollection<ObjectId> ids);
        Task Upsert(TEntity e);
        Task Upsert(IReadOnlyCollection<TEntity> entities);

        [Obsolete("Replaced with Upsert")]
        Task Insert(TEntity e);

        [Obsolete("Replaced with Upsert")]
        Task Insert(IReadOnlyCollection<TEntity> entities);

        [Obsolete("Replaced with Upsert")]
        Task Update(TEntity entity);

        [Obsolete("Replaced with Upsert")]
        Task Update(IReadOnlyCollection<TEntity> entities);

        Task Remove(TEntity entity);
        Task Remove(ObjectId id);
    }
}
