using System.Collections.Generic;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Recipe : IDocument
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public ObjectId RecipeTypeId { get; set; }
        public List<RecipeStep> RecipeSteps { get; set; }
        public List<RecipeItem> RecipeItems { get; set; }
        public ObjectId Id { get; set; }
        public string UserToken { get; set; }
    }
}
