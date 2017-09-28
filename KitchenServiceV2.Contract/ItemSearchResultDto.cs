using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class ItemSearchResultDto
    {
        [DataMember]
        public IEnumerable<ItemDto> Items { get; set; }

        [DataMember]
        public long TotalResults { get; set; }

        [DataMember]
        public long PageSize { get; set; }
    }
}
