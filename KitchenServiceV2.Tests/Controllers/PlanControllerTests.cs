using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Controllers;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Controllers
{
    public class PlanControllerTests : BaseControllerTests
    {
        private readonly PlanController _sut;

        public PlanControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new PlanController(this.PlanRepositoryMock.Object, this.ItemRepositoryMock.Object, this.RecipeRepositoryMock.Object);
            this.SetupController(this._sut);
        }

        [Fact]
        public async Task GetUpcomingPlansNoExistingShouldReturnStubs()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>());

            this.PlanRepositoryMock.Setup(x => x.GetOpenOrInRange(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Plan>());

            var plans = (await this._sut.GetUpcomingPlans(7)).ToList();
            Assert.Equal(7, plans.Count);

            var startDate = DateTimeOffset.Now.Date;
            for (int i = 0; i < 7; i++)
            {
                Assert.NotNull(plans.FirstOrDefault(x => x.DateTime == startDate && string.IsNullOrEmpty(x.Id)));
                startDate = startDate.AddDays(1);
            }
        }

        [Fact]
        public async Task GetUpcomingPlansAllExistingShouldReturn()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>());

            this.PlanRepositoryMock.Setup(x => x.GetOpenOrInRange(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Plan>
                {
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        DateTimeUnixSeconds = 0.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        DateTimeUnixSeconds = 1.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        DateTimeUnixSeconds = 2.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        DateTimeUnixSeconds = 3.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        DateTimeUnixSeconds = 4.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        DateTimeUnixSeconds = 5.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        DateTimeUnixSeconds = 6.DaysFromNow().ToUnixTimeSeconds()
                    }
                });

            var plans = (await this._sut.GetUpcomingPlans(7)).ToList();
            Assert.Equal(7, plans.Count);

            var startDate = DateTimeOffset.Now.Date;
            for (int i = 0; i < 7; i++)
            {
                Assert.NotNull(plans.FirstOrDefault(x => x.DateTime == startDate && !string.IsNullOrEmpty(x.Id)));
                startDate = startDate.AddDays(1);
            }
        }

        [Fact]
        public async Task GetUpcomingPlansIgnoreDone()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>());

            this.PlanRepositoryMock.Setup(x => x.GetOpenOrInRange(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Plan>
                {
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        PlanItems = new List<PlanItem>{new PlanItem{IsDone = true}},
                        DateTimeUnixSeconds = 0.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        PlanItems = new List<PlanItem>{new PlanItem{IsDone = true}},
                        DateTimeUnixSeconds = 1.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        PlanItems = new List<PlanItem>{new PlanItem{IsDone = true}},
                        DateTimeUnixSeconds = 2.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        PlanItems = new List<PlanItem>{new PlanItem{IsDone = true}},
                        DateTimeUnixSeconds = 3.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        PlanItems = new List<PlanItem>{new PlanItem{IsDone = true}},
                        DateTimeUnixSeconds = 4.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        PlanItems = new List<PlanItem>{new PlanItem{IsDone = true}},
                        DateTimeUnixSeconds = 5.DaysFromNow().ToUnixTimeSeconds()
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        PlanItems = new List<PlanItem>{new PlanItem{IsDone = true}},
                        DateTimeUnixSeconds = 6.DaysFromNow().ToUnixTimeSeconds()
                    }
                });

            var plans = (await this._sut.GetUpcomingPlans(7)).ToList();
            Assert.Equal(0, plans.Count);
        }

        [Fact]
        public async Task GetUpcomingPlansCorrectlyMaps()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>
                {
                    new Recipe
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "test recipe"
                    }
                });

            this.PlanRepositoryMock.Setup(x => x.GetOpenOrInRange(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(new List<Plan>
                {
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        IsDone = false,
                        PlanItems = new List<PlanItem>
                        {
                            new PlanItem
                            {
                                IsDone = false,
                                RecipeId = new ObjectId("599a98f185142b3ce0f96598")
                            }
                        },
                        DateTimeUnixSeconds = 0.DaysFromNow().ToUnixTimeSeconds()
                    }
                });

            var plans = (await this._sut.GetUpcomingPlans(1)).ToList();
            Assert.Equal(1, plans.Count);

            var planDto = plans.First();
            Assert.Equal("599a98f185142b3ce0f965a0", planDto.Id);
            Assert.Equal(0.DaysFromNow(), planDto.DateTime);
            Assert.Equal(1, planDto.Items.Count);
            Assert.Equal("599a98f185142b3ce0f96598", planDto.Items[0].RecipeId);
            Assert.Equal(false, planDto.Items[0].IsDone);
            Assert.Equal("test recipe", planDto.Items[0].RecipeName);
        }

        [Fact]
        public async Task PostCorrectlyMaps()
        {
            this.PlanRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((Plan) null);

            var dto = new PlanDto
            {
                Id = "599a98f185142b3ce0f965a0",
                DateTime = DateTimeOffset.Now.Date,
                Items = new List<PlanItemDto>
                {
                    new PlanItemDto
                    {
                        IsDone = false,
                        RecipeId = "599a98f185142b3ce0f96598",
                        RecipeName = "test recipe"
                    }
                }
            };

            this.PlanRepositoryMock.Setup(x => x.Upsert(It.IsAny<Plan>())).Returns(Task.CompletedTask);

            await this._sut.Post(dto);
            this.PlanRepositoryMock
                .Verify(x => x.Upsert(It.Is<Plan>(p =>
                        p.Id.ToString() == "599a98f185142b3ce0f965a0" && 
                        p.IsDone == false &&
                        p.DateTimeUnixSeconds == dto.DateTime.ToUnixTimeSeconds() && 
                        p.PlanItems.Count == 1 &&
                        p.PlanItems.Any(itm => itm.IsDone == false && itm.RecipeId.ToString() == "599a98f185142b3ce0f96598")
                )), Times.Once);
        }

        [Fact]
        public async Task PostCorrectlyMapsIsDone()
        {
            this.PlanRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((Plan)null);

            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>());

            var dto = new PlanDto
            {
                Id = "599a98f185142b3ce0f965a0",
                DateTime = DateTimeOffset.Now.Date,
                Items = new List<PlanItemDto>
                {
                    new PlanItemDto
                    {
                        IsDone = true,
                        RecipeId = "599a98f185142b3ce0f96598",
                        RecipeName = "test recipe"
                    }
                }
            };

            this.PlanRepositoryMock.Setup(x => x.Upsert(It.IsAny<Plan>())).Returns(Task.CompletedTask);

            await this._sut.Post(dto);
            this.PlanRepositoryMock
                .Verify(x => x.Upsert(It.Is<Plan>(p =>
                    p.Id.ToString() == "599a98f185142b3ce0f965a0" &&
                    p.IsDone &&
                    p.DateTimeUnixSeconds == dto.DateTime.ToUnixTimeSeconds() &&
                    p.PlanItems.Count == 1 &&
                    p.PlanItems.Any(itm => itm.IsDone && itm.RecipeId.ToString() == "599a98f185142b3ce0f96598")
                )), Times.Once);
        }

        [Fact]
        public async Task PostUpdateStock()
        {
            this.PlanRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync((Plan)null);

            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>
                {
                    new Recipe
                    {
                        Name = "test recipe",
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        RecipeItems = new List<RecipeItem>
                        {
                            new RecipeItem
                            {
                                ItemId = new ObjectId("599a98f185142b3ce0f96599"),
                                Amount = 1
                            },
                            new RecipeItem
                            {
                                ItemId = new ObjectId("599a98f185142b3ce0f9659b"),
                                Amount = 2
                            },
                            new RecipeItem
                            {
                                ItemId = new ObjectId("599a98f185142b3ce0f9659c"),
                                Amount = 20
                            }
                        }
                    }
                });

            this.ItemRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Item>
                {
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96599"),
                        Quantity = 10
                    },
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f9659b"),
                        Quantity = 10
                    },
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f9659c"),
                        Quantity = 10
                    }
                });

            this.ItemRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Item>>())).Returns(Task.CompletedTask);

            var dto = new PlanDto
            {
                Id = "599a98f185142b3ce0f965a0",
                DateTime = DateTimeOffset.Now.Date,
                Items = new List<PlanItemDto>
                {
                    new PlanItemDto
                    {
                        IsDone = true,
                        RecipeId = "599a98f185142b3ce0f96598",
                        RecipeName = "test recipe"
                    },
                    new PlanItemDto
                    {
                        IsDone = false,
                        RecipeId = "599a98f185142b3ce0f96599",
                        RecipeName = "test recipe 2"
                    }
                }
            };

            this.PlanRepositoryMock.Setup(x => x.Upsert(It.IsAny<Plan>())).Returns(Task.CompletedTask);

            await this._sut.Post(dto);

            this.ItemRepositoryMock
                .Verify(x => x.Upsert(It.Is<IReadOnlyCollection<Item>>(items =>
                    items.Count == 3 &&
                    items.Any(itm => itm.Id.ToString() == "599a98f185142b3ce0f96599" && itm.Quantity == 9) &&
                    items.Any(itm => itm.Id.ToString() == "599a98f185142b3ce0f9659b" && itm.Quantity == 8) &&
                    items.Any(itm => itm.Id.ToString() == "599a98f185142b3ce0f9659c" && itm.Quantity == 0)
                )), Times.Once);
        }
    }
}
