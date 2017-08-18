using System.Collections.Generic;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Category : IDocument
    {
        public string Name { get; set; }
        public List<ObjectId> ItemIds { get; set; }
        public ObjectId Id { get; set; }
    }
}
