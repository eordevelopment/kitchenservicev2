using System.Collections.Generic;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Category : IDocument
    {
        public Category()
        {
            this.ItemIds = new List<ObjectId>();
        }
        public string Name { get; set; }
        public List<ObjectId> ItemIds { get; set; }
        public ObjectId Id { get; set; }
        public string UserToken { get; set; }
    }
}
