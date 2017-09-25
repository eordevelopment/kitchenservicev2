using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class AuthResponseDto
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public string TokenType { get; set; }
    }
}
