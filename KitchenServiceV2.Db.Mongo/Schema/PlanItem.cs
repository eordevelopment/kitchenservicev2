using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class PlanItem
    {
        public bool IsDone { get; set; }
        public ObjectId RecipeId { get; set; }
    }
}
