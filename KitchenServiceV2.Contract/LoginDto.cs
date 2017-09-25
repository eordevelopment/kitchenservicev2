using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class LoginDto
    {
        [DataMember]
        public string IdToken { get; set; }
    }
}
