using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

            var openPlans = await this._planRepository.GetOpen(LoggedInUserToken, startDate, endDate);
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
            
        }
    }
}
