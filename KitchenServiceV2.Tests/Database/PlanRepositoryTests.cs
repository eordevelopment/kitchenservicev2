using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class PlanRepositoryTests : BaseDatabaseTests
    {
        private readonly IPlanRepository _sut;

        public PlanRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new PlanRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanFindByTokenAndDate()
        {
            var dto = DateTimeOffset.Now;

            var plan = new Plan
            {
                DateTimeUnixSeconds = dto.ToUnixTimeSeconds(),
                UserToken = "UserToken",
                IsDone = false,
                PlanItems = new List<PlanItem>()
            };

            await this._sut.Upsert(plan);
            Assert.NotNull(plan.Id);

            var dbItem = await this._sut.Find("UserToken", dto);
            Assert.NotNull(dbItem);
        }

        [Theory]
        [InlineData(-10, 10, 4)]
        [InlineData(-4, 10, 4)]
        [InlineData(-3, 10, 4)]
        [InlineData(-2, 10, 3)]
        [InlineData(1, 10, 3)]
        [InlineData(2, 10, 3)]
        [InlineData(3, 10, 2)]
        [InlineData(-10, 2, 4)]
        [InlineData(-10, 1, 3)]
        [InlineData(-10, 0, 3)]
        [InlineData(-10, -3, 3)]
        [InlineData(-10, -4, 2)]
        [InlineData(-10, -5, 2)]
        public async Task CanGetOpenAsync(int startOffset, int endOffset, int expected)
        {
            var start = DateTimeOffset.UtcNow.AddDays(startOffset);
            var end = DateTimeOffset.UtcNow.AddDays(endOffset);
            var plans = new List<Plan>
            {
                new Plan
                {
                    DateTimeUnixSeconds = DateTimeOffset.UtcNow.AddDays(-4).ToUnixTimeSeconds(),
                    UserToken = "UserToken",
                    IsDone = false,
                    PlanItems = new List<PlanItem>()
                },
                new Plan
                {
                    DateTimeUnixSeconds = DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds(),
                    UserToken = "UserToken",
                    IsDone = true,
                    PlanItems = new List<PlanItem>()
                },
                new Plan
                {
                    DateTimeUnixSeconds = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                    UserToken = "UserToken",
                    IsDone = false,
                    PlanItems = new List<PlanItem>()
                },
                new Plan
                {
                    DateTimeUnixSeconds = DateTimeOffset.UtcNow.AddDays(2).ToUnixTimeSeconds(),
                    UserToken = "UserToken",
                    IsDone = true,
                    PlanItems = new List<PlanItem>()
                }
            };

            await this._sut.Upsert(plans);
            var dbPlans = await this._sut.GetOpenOrInRange("UserToken", start, end);
            Assert.Equal(expected, dbPlans.Count);
        }

        [Fact]
        public async Task CanGetClosedPlans()
        {
            var plans = new List<Plan>();
            var dto = DateTimeOffset.UtcNow;

            for (var i = 0; i < 50; i++)
            {
                plans.Add(new Plan
                {
                    DateTimeUnixSeconds = dto.AddDays(-i).ToUnixTimeSeconds(),
                    UserToken = "UserToken",
                    IsDone = true,
                    PlanItems = new List<PlanItem>()
                });
            }

            await this._sut.Upsert(plans);

            var dbPlans = await this._sut.GetClosed("UserToken", 0, 10);
            Assert.Equal(10, dbPlans.Count);

            int offSet = 0;
            foreach (var dbPlan in dbPlans.OrderByDescending(x => x.DateTimeUnixSeconds))
            {
                var timeStamp = DateTimeOffset.FromUnixTimeSeconds(dbPlan.DateTimeUnixSeconds);
                var expected = dto.AddDays(-offSet);
                Assert.Equal(expected.ToUnixTimeSeconds(), timeStamp.ToUnixTimeSeconds());

                offSet += 1;
            }

            dbPlans = await this._sut.GetClosed("UserToken", 1, 10);
            Assert.Equal(10, dbPlans.Count);

            offSet = 10;
            foreach (var dbPlan in dbPlans.OrderByDescending(x => x.DateTimeUnixSeconds))
            {
                var timeStamp = DateTimeOffset.FromUnixTimeSeconds(dbPlan.DateTimeUnixSeconds);
                var expected = dto.AddDays(-offSet);
                Assert.Equal(expected.ToUnixTimeSeconds(), timeStamp.ToUnixTimeSeconds());

                offSet += 1;
            }
        }

        [Fact]
        public async Task CanGetPlansByRecipe()
        {
            var plan = new Plan
            {
                DateTimeUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                IsDone = false,
                UserToken = "UserToken",
                PlanItems = new List<PlanItem>
                {
                    new PlanItem
                    {
                        IsDone = false,
                        RecipeId = new ObjectId("599a98f185142b3ce0f965a0")
                    }
                }
            };

            await this._sut.Upsert(plan);

            var result = await this._sut.GetRecipePlans(new ObjectId("599a98f185142b3ce0f965a0"));
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }
    }
}
