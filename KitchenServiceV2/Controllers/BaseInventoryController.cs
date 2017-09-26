using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;

namespace KitchenServiceV2.Controllers
{
    public class BaseInventoryController : BaseController
    {
        protected readonly IPlanRepository PlanRepository;
        protected readonly IItemRepository ItemRepository;
        protected readonly IRecipeRepository RecipeRepository;

        public BaseInventoryController(IPlanRepository planRepository, IItemRepository itemRepository, IRecipeRepository recipeRepository)
        {
            this.PlanRepository = planRepository;
            this.ItemRepository = itemRepository;
            this.RecipeRepository = recipeRepository;
        }

        protected Task<List<Recipe>> GetPlanRecipes(IEnumerable<Plan> plans)
        {
            var recipeIds = plans.Where(x => x.PlanItems != null).SelectMany(x => x.PlanItems).Select(x => x.RecipeId).Distinct().ToList();
            if (recipeIds.Any())
            {
                return this.RecipeRepository.Get(recipeIds);
            }
            return Task.FromResult(new List<Recipe>());
        }

        protected Task<List<Item>> GetItems(List<Recipe> recipes, IEnumerable<ObjectId> additionalItems = null)
        {
            if (recipes.Any())
            {
                var recipeItems = recipes.SelectMany(x => x.RecipeItems).ToList();
                var itemIds = recipeItems.Select(x => x.ItemId).Distinct().ToList();
                if(additionalItems != null) itemIds.AddRange(additionalItems);
                return this.ItemRepository.Get(itemIds);
            }
            return Task.FromResult(new List<Item>());
        }
    }
}
