using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class RecipeItemDto
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public float Quantity { get; set; }

        [DataMember]
        public string UnitType { get; set; }

        [DataMember]
        public float Amount { get; set; }

        [DataMember]
        public string Instructions { get; set; }

        [DataMember]
        public string ItemId { get; set; }
    }
}
