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
            if (ids == null || !ids.Any()) return Task.FromResult(new List<TEntity>());
            return this.Collection.Find(p => ids.Contains(p.Id)).ToListAsync();
        }

        public Task Upsert(TEntity e)
        {
            if (e.Id == ObjectId.Empty)
            {
                return this.Insert(e);
            }
            return this.Update(e);
        }

        public async Task Upsert(IReadOnlyCollection<TEntity> entities)
        {
            var newItems = entities.Where(x => x.Id == ObjectId.Empty).ToList();
            var existingItems = entities.Where(x => x.Id != ObjectId.Empty).ToList();

            if (newItems.Any())
            {
                await this.Insert(newItems);
            }
            if (existingItems.Any())
            {
                await this.Update(existingItems);
            }
        }

        public Task Remove(TEntity entity)
        {
            return this.Remove(entity.Id);
        }

        public Task Remove(ObjectId id)
        {
            return this.Collection.DeleteOneAsync(p => p.Id == id);
        }

        private Task Insert(TEntity e)
        {
            return this.Collection.InsertOneAsync(e);
        }

        private Task Insert(IEnumerable<TEntity> entities)
        {
            return this.Collection.InsertManyAsync(entities);
        }

        private Task Update(TEntity entity)
        {
            return this.Collection.ReplaceOneAsync(p => p.Id == entity.Id, entity);
        }

        private async Task Update(IEnumerable<TEntity> entities)
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
    }
}
