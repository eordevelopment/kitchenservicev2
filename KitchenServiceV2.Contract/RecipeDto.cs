using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KitchenServiceV2.Contract
{
    [DataContract]
    public class RecipeDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public long TypeId { get; set; }

        [DataMember]
        public RecipeTypeDto RecipeType { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public List<RecipeStepDto> RecipeSteps { get; set; }
        [DataMember]
        public List<RecipeItemDto> RecipeItems { get; set; }
        [DataMember]
        public List<PlanDto> AssignedPlans { get; set; }
    }
}
