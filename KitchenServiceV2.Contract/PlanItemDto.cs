using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class PlanItemDto
    {
        [DataMember]
        public bool IsDone { get; set; }

        [DataMember]
        public string RecipeId { get; set; }

        [DataMember]
        public string RecipeName { get; set; }
    }
}
