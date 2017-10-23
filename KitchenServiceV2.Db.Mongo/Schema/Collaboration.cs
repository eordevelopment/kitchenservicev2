using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Collaboration : IDocument
    {
        public ObjectId Id { get; set; }
        public string UserToken { get; set; }
        public List<Collaborator> Collaborators { get; set; }
    }
}
