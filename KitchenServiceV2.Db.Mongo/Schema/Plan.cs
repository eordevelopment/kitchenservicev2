using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace KitchenServiceV2.Db.Mongo.Schema
{
    public class Plan : IDocument
    {
        public DateTimeOffset? DateTime { get; set; }
        public bool IsDone { get; set; }
        public List<PlanItem> PlanItems { get; set; }
        public ObjectId Id { get; set; }
        public string UserToken { get; set; }
    }
}
