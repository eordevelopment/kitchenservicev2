using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class RecipeItemDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public float Amount { get; set; }

        [DataMember]
        public string Instructions { get; set; }

        [DataMember]
        public ItemDto Item { get; set; }
    }
}
