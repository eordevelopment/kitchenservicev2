namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class RecipeItem
    {
        public string Name { get; set; }
        public float Quantity { get; set; }
        public string UnitType { get; set; }
        public float Amount { get; set; }
        public string Instructions { get; set; }
    }
}
