using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class CategoryDto
    {
        public CategoryDto()
        {
            this.Items = new List<ItemDto>();
        }
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<ItemDto> Items { get; set; }
    }
}
