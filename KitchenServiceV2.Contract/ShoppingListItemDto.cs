using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    using System.Collections.Generic;

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

        [DataMember]
        public IEnumerable<RecipeDto> Recipes { get; set; }
    }
}
