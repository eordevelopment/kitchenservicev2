using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class AccountDto
    {
        [DataMember]
        public string IdToken { get; set; }
    }
}
