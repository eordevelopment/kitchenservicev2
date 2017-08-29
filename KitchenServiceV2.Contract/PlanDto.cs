using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class PlanDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public List<PlanItemDto> Items { get; set; }
    }
}
