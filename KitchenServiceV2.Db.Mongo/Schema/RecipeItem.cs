using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class RecipeItem
    {
        public float Amount { get; set; }
        public string Instructions { get; set; }

        public ObjectId ItemId { get; set; }
    }
}
