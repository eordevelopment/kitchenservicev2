using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class ItemToBuy : IDocument
    {
        public ObjectId Id { get; set; }
        public string UserToken { get; set; }

        public ObjectId ItemId { get; set; }
    }
}
