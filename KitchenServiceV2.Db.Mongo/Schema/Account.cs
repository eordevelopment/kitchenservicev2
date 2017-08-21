using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Account : IDocument
    {
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public ObjectId Id { get; set; }
        public string UserToken { get; set; }
    }
}
