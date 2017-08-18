using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class AccountDto
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string HashedPassword { get; set; }

        [DataMember]
        public string Token { get; set; }
    }
}
