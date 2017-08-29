using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "HasToken")]
    public class PlanController : BaseController
    {
        private readonly IPlanRepository _planRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IRecipeRepository _recipeRepository;

        public PlanController(IPlanRepository planRepository, IItemRepository itemRepository, IRecipeRepository recipeRepository)
        {
            this._planRepository = planRepository;
            this._itemRepository = itemRepository;
            this._recipeRepository = recipeRepository;
        }

        [HttpGet("/api/plan/upcoming/{number}")]
        public async Task<IEnumerable<PlanDto>> GetUpcomingPlans(int number)
        {
            var startDate = DateTimeOffset.Now.Date;
            var endDate = startDate.AddDays(number);

            var openPlans = await this._planRepository.GetOpenOrInRange(LoggedInUserToken, startDate, endDate);
            var planDtos = openPlans.Select(Mapper.Map<PlanDto>).ToList();
            var dt = DateTimeOffset.UtcNow;

            while (planDtos.Count < number)
            {
                if (planDtos.All(x => x.DateTime.Date != dt.Date))
                {
                    planDtos.Add(new PlanDto
                    {
                        DateTime = dt.Date,
                        Items = new List<PlanItemDto>()
                    });
                }
                dt = dt.AddDays(1);
            }
            return planDtos;
        }

        [HttpGet("/api/plan/closed/{page}")]
        public async Task<List<PlanDto>> GetClosedPlans(int page = 0)
        {
            var plans = await this._planRepository.GetClosed(LoggedInUserToken, page, 10);
            return plans.Select(Mapper.Map<PlanDto>).ToList();
        }

        [HttpPost]
        public async Task Post([FromBody] PlanDto value)
        {
            if (value.DateTime == null || value.DateTime == DateTimeOffset.MinValue)
            {
                throw new InvalidOperationException("Invalid date");
            }
            var existingPlan = await this._planRepository.Find(LoggedInUserToken, value.DateTime);
            if (existingPlan != null)
            {
                throw new InvalidOperationException("Plan already exists.");
            }

            var plan = Mapper.Map<Plan>(value);
            plan.UserToken = LoggedInUserToken;

            var cookedMeals = plan.PlanItems.Where(x => x.IsDone);
            await this.UpdateStock(cookedMeals, null);

            await this._planRepository.Upsert(plan);
        }

        [HttpPut("{id}")]
        public async Task Put(string id, [FromBody] PlanDto value)
        {
            if (value.DateTime == null || value.DateTime == DateTimeOffset.MinValue)
            {
                throw new InvalidOperationException("Invalid date");
            }

            var existingPlan = await this._planRepository.Find(LoggedInUserToken, value.DateTime);
            if (existingPlan != null && existingPlan.Id.ToString() != id)
            {
                throw new InvalidOperationException("Plan already exists.");
            }

            var plan = await this._planRepository.Get(new ObjectId(id));
            if (plan == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }

            var updatedPlan = Mapper.Map<Plan>(value);
            updatedPlan.Id = plan.Id;
            updatedPlan.UserToken = plan.UserToken;
            await this.UpdateStock(updatedPlan.PlanItems.Where(x => x.IsDone), plan.PlanItems);

            await this._planRepository.Upsert(plan);
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var planId = new ObjectId(id);
            var plan = await this._planRepository.Get(planId);
            if (plan == null)
            {
                throw new ArgumentException($"No resource with id: {id}");
            }
            await this._planRepository.Remove(planId);
        }

        [NonAction]
        private async Task UpdateStock(IEnumerable<PlanItem> updatedItems, IReadOnlyCollection<PlanItem> originalItems)
        {
            //First, find all the recipes that have been updated
            var recipesToUpdate = (from planItem in updatedItems
                                   let originalItem = originalItems?.FirstOrDefault(x => x.RecipeId == planItem.RecipeId)
                                   where originalItem == null || !originalItem.IsDone
                                   select planItem.RecipeId).Distinct().ToList();

            var recipes = await this._recipeRepository.Get(recipesToUpdate);

            //Next, find all the items that need to be updated
            var recipeItems = recipes.SelectMany(x => x.RecipeItems).ToList();
            var itemIds = recipeItems.Select(x => x.ItemId).Distinct().ToList();

            var itemsById = (await this._itemRepository.Get(itemIds)).ToDictionary(x => x.Id);

            //Finally, update the stock
            foreach (var recipeItem in recipeItems)
            {
                if(!itemsById.ContainsKey(recipeItem.ItemId)) continue;

                var item = itemsById[recipeItem.ItemId];
                item.Quantity -= recipeItem.Amount;
                if (item.Quantity < 0) item.Quantity = 0;
            }

            await this._itemRepository.Upsert(itemsById.Values);
        }
    }
}
