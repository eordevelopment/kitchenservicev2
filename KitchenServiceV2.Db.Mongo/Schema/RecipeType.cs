using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class RecipeType : IDocument
    {
        public string Name { get; set; }
        public ObjectId Id { get; set; }
        public string UserToken { get; set; }
    }
}
