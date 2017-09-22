using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PlanController : BaseInventoryController
    {
        public PlanController(IPlanRepository planRepository, IItemRepository itemRepository, IRecipeRepository recipeRepository)
            :base(planRepository, itemRepository, recipeRepository)
        {
        }

        [HttpGet("/api/plan/upcoming/{number}")]
        public async Task<IEnumerable<PlanDto>> GetUpcomingPlans(int number)
        {
            var startDate = DateTimeOffset.UtcNow.Date;
            var endDate = startDate.AddDays(number);

            var openPlans = await this.PlanRepository.GetOpenOrInRange(LoggedInUserToken, startDate, endDate);
            var planDtos = openPlans.Select(Mapper.Map<PlanDto>).ToList();
            var dt = DateTimeOffset.UtcNow;

            while (planDtos.Count < number)
            {
                if (planDtos.All(x => x.DateTime != dt.Date))
                {
                    planDtos.Add(new PlanDto
                    {
                        DateTime = dt.Date,
                        Items = new List<PlanItemDto>()
                    });
                }
                dt = dt.AddDays(1);
            }
            var validPlans = planDtos
                .Where(x => x.Items == null || x.Items.Any(y => !y.IsDone) || !x.Items.Any())
                .OrderBy(x => x.DateTime)
                .ToList();

            await this.SetRecipeNames(validPlans, openPlans);

            return validPlans;
        }

        [HttpGet("/api/plan/closed/{page}")]
        public async Task<List<PlanDto>> GetClosedPlans(int page = 0)
        {
            var plans = await this.PlanRepository.GetClosed(LoggedInUserToken, page, 10);
            var dtos = plans.Select(Mapper.Map<PlanDto>).ToList();

            await this.SetRecipeNames(dtos, plans);

            return dtos;
        }

        [HttpPost]
        public async Task Post([FromBody] PlanDto value)
        {
            if (value.DateTime == null || value.DateTime == DateTimeOffset.MinValue)
            {
                throw new InvalidOperationException("Invalid date");
            }
            var existingPlan = await this.PlanRepository.Find(LoggedInUserToken, value.DateTime);
            if (existingPlan != null)
            {
                throw new InvalidOperationException("Plan already exists.");
            }

            var plan = Mapper.Map<Plan>(value);
            plan.UserToken = LoggedInUserToken;

            var cookedMeals = plan.PlanItems.Where(x => x.IsDone);
            await this.UpdateStock(cookedMeals, null);

            await this.PlanRepository.Upsert(plan);
        }

        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] PlanDto value)
        {
            if (value.DateTime == null || value.DateTime == DateTimeOffset.MinValue)
            {
                throw new InvalidOperationException("Invalid date");
            }

            var existingPlan = await this.PlanRepository.Find(LoggedInUserToken, value.DateTime);
            if (existingPlan != null && existingPlan.Id.ToString() != id)
            {
                throw new InvalidOperationException("Plan already exists.");
            }

            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var plan = await this.PlanRepository.Get(objectId);
            if (plan == null || plan.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            var updatedPlan = Mapper.Map<Plan>(value);
            updatedPlan.Id = plan.Id;
            updatedPlan.UserToken = plan.UserToken;
            await this.UpdateStock(updatedPlan.PlanItems.Where(x => x.IsDone), plan.PlanItems);

            await this.PlanRepository.Upsert(updatedPlan);
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var objectId = Mapper.Map<ObjectId>(id);
            if (objectId == ObjectId.Empty) throw new ArgumentException($"Invalid id: {id}");

            var plan = await this.PlanRepository.Get(objectId);
            if (plan == null || plan.UserToken != LoggedInUserToken)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }
            await this.PlanRepository.Remove(objectId);
        }

        [NonAction]
        private async Task UpdateStock(IEnumerable<PlanItem> updatedItems, IReadOnlyCollection<PlanItem> originalItems)
        {
            //First, find all the recipes that have been updated
            var recipesToUpdate = (from planItem in updatedItems
                                   let originalItem = originalItems?.FirstOrDefault(x => x.RecipeId == planItem.RecipeId)
                                   where planItem.IsDone && (originalItem == null || !originalItem.IsDone)
                                   select planItem.RecipeId).Distinct().ToList();

            if (recipesToUpdate.Any())
            {
                var recipes = await this.RecipeRepository.Get(recipesToUpdate);
                var items = await this.GetRecipeItems(recipes);

                if (items.Any())
                {
                    //Next, find all the items that need to be updated
                    var recipeItems = recipes.SelectMany(x => x.RecipeItems).ToList();
                    var itemsById = items.ToDictionary(x => x.Id);

                    //Finally, update the stock
                    foreach (var recipeItem in recipeItems)
                    {
                        if (!itemsById.ContainsKey(recipeItem.ItemId)) continue;

                        var item = itemsById[recipeItem.ItemId];
                        item.Quantity -= recipeItem.Amount;
                        if (item.Quantity < 0) item.Quantity = 0;
                    }

                    await this.ItemRepository.Upsert(itemsById.Values);
                }
            }
        }

        [NonAction]
        private async Task SetRecipeNames(IEnumerable<PlanDto> planDtos, IEnumerable<Plan> dbPlans)
        {
            var recipesById = (await this.GetPlanRecipes(dbPlans)).ToDictionary(x => x.Id.ToString());

            foreach (var planDto in planDtos.Where(x => x.Items != null))
            {
                foreach (var itemDto in planDto.Items)
                {
                    if (!recipesById.ContainsKey(itemDto.RecipeId)) continue;
                    itemDto.RecipeName = recipesById[itemDto.RecipeId].Name;
                }
            }
        }
    }
}
