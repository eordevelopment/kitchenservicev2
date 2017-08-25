using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class ShoppingListItem
    {
        public float Amount { get; set; }
        public float TotalAmount { get; set; }
        public bool IsDone { get; set; }
        public ObjectId ItemId { get; set; }
    }
}
