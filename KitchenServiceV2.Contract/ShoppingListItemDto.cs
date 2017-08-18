using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class ShoppingListItemDto
    {
        [DataMember]
        public float Amount { get; set; }

        [DataMember]
        public float TotalAmount { get; set; }

        [DataMember]
        public bool IsDone { get; set; }

        [DataMember]
        public ItemDto Item { get; set; }
    }
}
