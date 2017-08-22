using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KitchenServiceV2.Db.Mongo.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : IDocument
    {
        private readonly IDbContext _context;

        protected IMongoCollection<TEntity> Collection => this._context.Db.GetCollection<TEntity>(this.CollectionName);

        public Repository(IDbContext context, string collectionName)
        {
            this.CollectionName = collectionName;
            this._context = context;
        }

        public string CollectionName { get; }

        public Task<List<TEntity>> GetAll(string userToken)
        {
            return this.Collection.Find(p => p.UserToken == userToken).ToListAsync();
        }

        public Task<TEntity> Get(ObjectId id)
        {
            return this.Collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public Task<List<TEntity>> Get(IReadOnlyCollection<ObjectId> ids)
        {
            return this.Collection.Find(p => ids.Contains(p.Id)).ToListAsync();
        }

        public Task Insert(TEntity e)
        {
            return this.Collection.InsertOneAsync(e);
        }

        public Task Insert(IReadOnlyCollection<TEntity> entities)
        {
            return this.Collection.InsertManyAsync(entities);
        }

        public Task Update(TEntity entity)
        {
            return this.Collection.ReplaceOneAsync(p => p.Id == entity.Id, entity);
        }

        public async Task Update(IReadOnlyCollection<TEntity> entities)
        {
            var writeModels = new List<WriteModel<TEntity>>();
            foreach (var entity in entities)
            {
                var filter = new ExpressionFilterDefinition<TEntity>(x => x.Id == entity.Id);
                var replaceModel = new ReplaceOneModel<TEntity>(filter, entity);
                writeModels.Add(replaceModel);
            }
            await this.Collection.BulkWriteAsync(writeModels);
        }

        public Task Remove(TEntity entity)
        {
            return this.Remove(entity.Id);
        }

        public Task Remove(ObjectId id)
        {
            return this.Collection.DeleteOneAsync(p => p.Id == id);
        }
    }
}
