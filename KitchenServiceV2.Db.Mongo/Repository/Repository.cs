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
        private readonly string _collectionName;
        private readonly IDbContext _context;

        protected IMongoCollection<TEntity> Collection => this._context.Db.GetCollection<TEntity>(this._collectionName);

        public Repository(IDbContext context, string collectionName)
        {
            this._collectionName = collectionName;
            this._context = context;
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            var result = new List<TEntity>();
            using (var cursor = await this.Collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    result.AddRange(batch);
                }
            }

            return result;
        }

        public Task<TEntity> Get(ObjectId id)
        {
            return this.Collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public Task<List<TEntity>> Get(IReadOnlyCollection<ObjectId> ids)
        {
            return this.Collection.Find(p => ids.Contains(p.Id)).ToListAsync();
        }

        public Task Insert(TEntity p)
        {
            return this.Collection.InsertOneAsync(p);
        }

        public Task Update(TEntity entity)
        {
            return this.Collection.ReplaceOneAsync(p => p.Id == entity.Id, entity);
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
