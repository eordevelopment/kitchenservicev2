using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class ShoppingListDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsDone { get; set; }

        [DataMember]
        public DateTimeOffset CreatedOn { get; set; }

        [DataMember]
        public List<ShoppingListItemDto> Items { get; set; }

        [DataMember]
        public List<ShoppingListItemDto> OptionalItems { get; set; }

        [DataMember]
        public List<RecipeDto> Recipes { get; set; }
    }
}
