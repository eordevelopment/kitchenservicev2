using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Item : IDocument
    {
        public string Name { get; set; }
        public float Quantity { get; set; }
        public string UnitType { get; set; }
        public ObjectId Id { get; set; }
    }
}
