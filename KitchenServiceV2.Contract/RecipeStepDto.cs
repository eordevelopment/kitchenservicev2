using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class RecipeStepDto
    {
        [DataMember]
        public int StepNumber { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
