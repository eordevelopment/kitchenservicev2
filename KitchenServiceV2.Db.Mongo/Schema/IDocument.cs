using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public interface IDocument
    {
        ObjectId Id { get; set; }
    }
}
