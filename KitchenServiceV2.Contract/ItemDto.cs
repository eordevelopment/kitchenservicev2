using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class ItemDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public float Quantity { get; set; }

        [DataMember]
        public string UnitType { get; set; }

        [DataMember]
        public bool FlaggedForNextShop { get; set; }

        [DataMember]
        public IEnumerable<RecipeDto> Recipes { get; set; }
    }
}
