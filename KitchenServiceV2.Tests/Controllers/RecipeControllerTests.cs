using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            this._sut = new RecipeController(this.RecipeRepositoryMock.Object, this.RecipeTypeRepositoryMock.Object, this.ItemRepositoryMock.Object);
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

            Assert.Equal("item 1", result.RecipeItems[0].Name);
            Assert.Equal(1, result.RecipeItems[0].Quantity);
            Assert.Equal("1", result.RecipeItems[0].UnitType);
            Assert.Equal(10, result.RecipeItems[0].Amount);
            Assert.Equal("diced", result.RecipeItems[0].Instructions);
            Assert.Equal("599a98f185142b3ce0f96599", result.RecipeItems[0].ItemId);

            Assert.Equal("item 2", result.RecipeItems[1].Name);
            Assert.Equal(2, result.RecipeItems[1].Quantity);
            Assert.Equal("2", result.RecipeItems[1].UnitType);
            Assert.Equal(20, result.RecipeItems[1].Amount);
            Assert.Equal("chopped", result.RecipeItems[1].Instructions);
            Assert.Equal("599a98f185142b3ce0f9659b", result.RecipeItems[1].ItemId);
        }
    }
}
