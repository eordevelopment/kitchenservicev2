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
    public class RecipeControllerTests : BaseControllerTests
    {
        private readonly RecipeController _sut;

        public RecipeControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new RecipeController(
                this.RecipeRepositoryMock.Object, 
                this.RecipeTypeRepositoryMock.Object, 
                this.ItemRepositoryMock.Object,
                this.PlanRepositoryMock.Object);
            this.SetupController(this._sut);
        }

        [Fact]
        public async Task GetNoItemsReturnsEmpty()
        {
            this.RecipeRepositoryMock.Setup(x => x.GetAll(It.IsAny<string>())).ReturnsAsync((List<Recipe>)null);

            var result = await this._sut.Get();

            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task GetReturnsItems()
        {
            this.RecipeRepositoryMock.Setup(x => x.GetAll(It.IsAny<string>()))
                .ReturnsAsync(new List<Recipe>
                {
                    new Recipe
                    {
                        Name = "recipe1",
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        UserToken = "UserToken",
                        Key = "recipeKey1",
                        RecipeTypeId = new ObjectId("599a98f185142b3ce0f96599")
                    },
                    new Recipe
                    {
                        Name = "recipe2",
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        UserToken = "UserToken",
                        Key = "recipeKey2",
                        RecipeTypeId = new ObjectId("599a98f185142b3ce0f9659b")
                    }
                });

            this.RecipeTypeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<RecipeType>
                {
                    new RecipeType
                    {
                        Name = "type 1",
                        Id = new ObjectId("599a98f185142b3ce0f96599"),
                        UserToken = "UserToken"
                    }
                });

            var result = await this._sut.Get();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var item1 = result[0];
            Assert.Equal("recipe1", item1.Name);
            Assert.Equal("599a98f185142b3ce0f965a0", item1.Id);
            Assert.Equal("recipeKey1", item1.Key);
            Assert.NotNull(item1.RecipeType);
            Assert.Equal("599a98f185142b3ce0f96599", item1.RecipeType.Id);

            var item2 = result[1];
            Assert.Equal("recipe2", item2.Name);
            Assert.Equal("599a98f185142b3ce0f96598", item2.Id);
            Assert.Equal("recipeKey2", item2.Key);
            Assert.Null(item2.RecipeType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetNoIdShouldThrow(string id)
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Get(id));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal("Please provide an id", exception.Message);
            }
        }

        [Fact]
        public async Task GetNotFoundShouldThrow()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((Recipe)null);

            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Get("599a98f185142b3ce0f9659c"));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal("No resource with id: 599a98f185142b3ce0f9659c", exception.Message);
            }
        }

        [Fact]
        public async Task GetShouldMapRecipe()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new Recipe
                {
                    Name = "recipe1",
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    UserToken = "UserToken",
                    RecipeTypeId = new ObjectId("599a98f185142b3ce0f96598"),
                    Key = "RecipeKey",
                    RecipeSteps = new List<RecipeStep>
                    {
                        new RecipeStep { StepNumber = 1, Description = "Step 1." },
                        new RecipeStep { StepNumber = 2, Description = "Step 2." }
                    },
                    RecipeItems = new List<RecipeItem>
                    {
                        new RecipeItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96599"),
                            Instructions = "diced",
                            Amount = 10
                        },
                        new RecipeItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f9659b"),
                            Instructions = "chopped",
                            Amount = 20
                        }
                    }
                });

            this.RecipeTypeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new RecipeType
                {
                    Id = new ObjectId("599a98f185142b3ce0f96598"),
                    Name = "recipe type",
                    UserToken = "UserToken"
                });

            this.ItemRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Item>
                {
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96599"),
                        Name = "item 1",
                        Quantity = 1,
                        UnitType = "1"
                    },
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f9659b"),
                        Name = "item 2",
                        Quantity = 2,
                        UnitType = "2"
                    }
                });

            this.PlanRepositoryMock.Setup(x => x.GetRecipePlans(It.IsAny<ObjectId>()))
                .ReturnsAsync(new List<Plan>());

            var result = await this._sut.Get("599a98f185142b3ce0f965a0");

            Assert.NotNull(result);
            Assert.Equal("recipe1", result.Name);
            Assert.Equal("599a98f185142b3ce0f965a0", result.Id);
            Assert.Equal("RecipeKey", result.Key);
            Assert.NotNull(result.RecipeType);
            Assert.Equal("599a98f185142b3ce0f96598", result.RecipeType.Id);
            Assert.Equal("recipe type", result.RecipeType.Name);

            Assert.Equal(2, result.RecipeSteps.Count);

            Assert.Equal("Step 1.", result.RecipeSteps[0].Description);
            Assert.Equal(1, result.RecipeSteps[0].StepNumber);

            Assert.Equal("Step 2.", result.RecipeSteps[1].Description);
            Assert.Equal(2, result.RecipeSteps[1].StepNumber);

            Assert.Equal(2, result.RecipeItems.Count);

            Assert.Equal("item 1", result.RecipeItems[0].Item.Name);
            Assert.Equal(1, result.RecipeItems[0].Item.Quantity);
            Assert.Equal("1", result.RecipeItems[0].Item.UnitType);
            Assert.Equal(10, result.RecipeItems[0].Amount);
            Assert.Equal("diced", result.RecipeItems[0].Instructions);
            Assert.Equal("599a98f185142b3ce0f96599", result.RecipeItems[0].Item.Id);

            Assert.Equal("item 2", result.RecipeItems[1].Item.Name);
            Assert.Equal(2, result.RecipeItems[1].Item.Quantity);
            Assert.Equal("2", result.RecipeItems[1].Item.UnitType);
            Assert.Equal(20, result.RecipeItems[1].Amount);
            Assert.Equal("chopped", result.RecipeItems[1].Instructions);
            Assert.Equal("599a98f185142b3ce0f9659b", result.RecipeItems[1].Item.Id);
        }

        [Fact]
        public async Task DeleteNotFound()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((Recipe)null);
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Delete("599a98f185142b3ce0f9659c"));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal("No resource with id: 599a98f185142b3ce0f9659c", exception.Message);
            }
        }

        [Fact]
        public async Task DeleteShouldDelete()
        {
            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync(new Recipe
            {
                UserToken = "UserToken"
            });
            this.RecipeRepositoryMock.Setup(x => x.Remove(It.IsAny<ObjectId>())).Returns(Task.CompletedTask);

            await this._sut.Delete("599a98f185142b3ce0f9659c");

            this.RecipeRepositoryMock.Verify(x => x.Remove(It.Is<ObjectId>(y => y.ToString() == "599a98f185142b3ce0f9659c")), Times.Once);
        }

        [Fact]
        public async Task PostNoNameShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(new RecipeDto()));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PutNoNameShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("599a98f185142b3ce0f965a0", new RecipeDto()));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostNoItemNameShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(new RecipeDto
            {
                Name = "Test",
                RecipeItems = new List<RecipeItemDto>
                {
                    new RecipeItemDto
                    {
                        Item = new ItemDto()
                    }
                }
            }));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Item name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PutNoItemNameShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("599a98f185142b3ce0f965a0", new RecipeDto
            {
                Name = "Test",
                RecipeItems = new List<RecipeItemDto>
                {
                    new RecipeItemDto
                    {
                        Item = new ItemDto()
                    }
                }
            }));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Item name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostNoStepDescShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(new RecipeDto
            {
                Name = "Test",
                RecipeSteps = new List<RecipeStepDto>
                {
                    new RecipeStepDto()
                }
            }));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Description cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PutNoStepDescShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("599a98f185142b3ce0f965a0", new RecipeDto
            {
                Name = "Test",
                RecipeSteps = new List<RecipeStepDto>
                {
                    new RecipeStepDto()
                }
            }));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Description cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostShouldSave()
        {
            var recipe = new RecipeDto
            {
                Name = "Test Recipe",
                RecipeType = new RecipeTypeDto
                {
                    Id = "599a98f185142b3ce0f965a0"
                },
                RecipeSteps = new List<RecipeStepDto>
                {
                    new RecipeStepDto
                    {
                        Description = "Step 1",
                        StepNumber = 1
                    },
                    new RecipeStepDto
                    {
                        Description = "Step 2",
                        StepNumber = 2
                    }
                },
                RecipeItems = new List<RecipeItemDto>
                {
                    new RecipeItemDto
                    {
                        Amount = 1,
                        Instructions = "diced",
                        Item = new ItemDto
                        {
                            Name = "Existing Item 1",
                            Id = "599a98f185142b3ce0f9659b",
                            Quantity = 10,
                            UnitType = "ml"
                        }
                    },
                    new RecipeItemDto
                    {
                        Amount = 2,
                        Instructions = "diced",
                        Item = new ItemDto
                        {
                            Name = "Existing Item 2",
                            Id = "599a98f185142b3ce0f96599",
                            Quantity = 20,
                            UnitType = "ml"
                        }
                    },
                    new RecipeItemDto
                    {
                        Amount = 3,
                        Instructions = "chopped",
                        Item = new ItemDto
                        {
                            Name = "New Item 1",
                            Quantity = 30,
                            UnitType = "kg"
                        }
                    },
                    new RecipeItemDto
                    {
                        Amount = 4,
                        Instructions = "chopped",
                        Item = new ItemDto
                        {
                            Name = "New Item 2",
                            Quantity = 40,
                            UnitType = "kg"
                        }
                    }
                }
            };

            this.RecipeRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Recipe) null);
            this.ItemRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Item>>())).Returns(Task.CompletedTask);
            this.RecipeRepositoryMock.Setup(x => x.Upsert(It.IsAny<Recipe>())).Returns(Task.CompletedTask);

            await this._sut.Post(recipe);

            this.ItemRepositoryMock
                .Verify(x => x.Upsert(It.Is<IReadOnlyCollection<Item>>(items =>
                    items.Count == 4 &&
                    items.Any(itm => itm.Name == "new item 1" && itm.Quantity == 30 && itm.UnitType == "kg" && itm.UserToken == "UserToken") &&
                    items.Any(itm => itm.Name == "new item 2" && itm.Quantity == 40 && itm.UnitType == "kg" && itm.UserToken == "UserToken") &&
                    items.Any(itm => itm.Id.ToString() == "599a98f185142b3ce0f9659b" && itm.Name == "existing item 1" && itm.Quantity == 10 && itm.UnitType == "ml" && itm.UserToken == "UserToken") &&
                    items.Any(itm => itm.Id.ToString() == "599a98f185142b3ce0f96599" && itm.Name == "existing item 2" && itm.Quantity == 20 && itm.UnitType == "ml" && itm.UserToken == "UserToken")
                )), Times.Once);

            this.RecipeRepositoryMock
                .Verify(x => x.Upsert(It.Is<Recipe>(r =>
                    r.Name == "test recipe" &&
                    r.Key != null &&
                    r.UserToken == "UserToken" &&
                    r.RecipeSteps.Count == 2 &&
                    r.RecipeSteps.Any(s => s.Description == "Step 1" && s.StepNumber == 1) &&
                    r.RecipeSteps.Any(s => s.Description == "Step 2" && s.StepNumber == 2) &&
                    r.RecipeItems.Count == 4 &&
                    r.RecipeItems.Any(i => i.ItemId.ToString() == "599a98f185142b3ce0f9659b" && i.Amount == 1 && i.Instructions == "diced") &&
                    r.RecipeItems.Any(i => i.ItemId.ToString() == "599a98f185142b3ce0f96599" && i.Amount == 2 && i.Instructions == "diced") &&
                    r.RecipeItems.Any(i => i.Amount == 3 && i.Instructions == "chopped") &&
                    r.RecipeItems.Any(i => i.Amount == 4 && i.Instructions == "chopped")
                )), Times.Once);
        }
    }
}
