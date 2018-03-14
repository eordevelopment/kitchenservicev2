using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    using System.Collections.Generic;

    public class ShoppingListItem
    {
        public ShoppingListItem()
        {
            this.RecipeIds = new HashSet<ObjectId>();
        }

        public float Amount { get; set; }
        public float TotalAmount { get; set; }
        public bool IsDone { get; set; }
        public ObjectId ItemId { get; set; }
        public HashSet<ObjectId> RecipeIds { get; set; }
    }
}
