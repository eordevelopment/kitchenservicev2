using System;
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

        public Task Upsert(TEntity e)
        {
            if (e.Id == ObjectId.Empty)
            {
                return this.Collection.InsertOneAsync(e);
            }
            return this.Collection.ReplaceOneAsync(p => p.Id == e.Id, e);
        }

        public Task Upsert(IReadOnlyCollection<TEntity> entities)
        {
            var newItems = entities.Where(x => x.Id == ObjectId.Empty);
            var existingItems = entities.Where(x => x.Id != ObjectId.Empty);

            if (newItems.Any())
            {
                return this.Collection.InsertManyAsync(entities);
            }
            if (existingItems.Any())
            {
                var writeModels = new List<WriteModel<TEntity>>();
                foreach (var entity in entities)
                {
                    var filter = new ExpressionFilterDefinition<TEntity>(x => x.Id == entity.Id);
                    var replaceModel = new ReplaceOneModel<TEntity>(filter, entity);
                    writeModels.Add(replaceModel);
                }
                return this.Collection.BulkWriteAsync(writeModels);
            }
            return Task.CompletedTask;
        }

        [Obsolete("Replaced with Upsert")]
        public Task Insert(TEntity e)
        {
            return this.Collection.InsertOneAsync(e);
        }

        [Obsolete("Replaced with Upsert")]
        public Task Insert(IReadOnlyCollection<TEntity> entities)
        {
            return this.Collection.InsertManyAsync(entities);
        }

        [Obsolete("Replaced with Upsert")]
        public Task Update(TEntity entity)
        {
            return this.Collection.ReplaceOneAsync(p => p.Id == entity.Id, entity);
        }

        [Obsolete("Replaced with Upsert")]
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
