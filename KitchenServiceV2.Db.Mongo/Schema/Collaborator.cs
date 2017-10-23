using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Collaborator
    {
        public string UserToken { get; set; }
        public string Email { get; set; }
        public int AccessLevel;
    }
}
