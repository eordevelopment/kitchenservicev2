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
    public class ListControllerTests : BaseControllerTests
    {
        private readonly ListController _sut;

        public ListControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new ListController(this.PlanRepositoryMock.Object, this.ItemRepositoryMock.Object, this.ShoppingListRepositoryMock.Object, this.RecipeRepositoryMock.Object);
            this.SetupController(this._sut);
        }

        [Fact]
        public async Task GetOpenCorrectlyMaps()
        {
            var now = DateTimeOffset.UtcNow;
            this.ShoppingListRepositoryMock.Setup(x => x.GetOpen(It.IsAny<string>()))
                .ReturnsAsync(new ShoppingList
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    Name = "Test List",
                    IsDone = false,
                    UserToken = "UserToken",
                    CreatedOnUnixSeconds = now.ToUnixTimeSeconds(),
                    Items = new List<ShoppingListItem>
                    {
                        new ShoppingListItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96598"),
                            IsDone = true,
                            Amount = 10,
                            TotalAmount = 100
                        },
                        new ShoppingListItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96599"),
                            IsDone = false,
                            Amount = 20,
                            TotalAmount = 200
                        }
                    },
                    OptionalItems = new List<ShoppingListItem>
                    {
                        new ShoppingListItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f9659b"),
                            IsDone = false,
                            Amount = 30,
                            TotalAmount = 300
                        },
                        new ShoppingListItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f9659c"),
                            IsDone = false,
                            Amount = 40,
                            TotalAmount = 500
                        }
                    }
                });

            this.ItemRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Item>
                {
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "item 1",
                        Quantity = 1000,
                        UnitType = "ml"
                    },
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96599"),
                        Name = "item 2",
                        Quantity = 2000,
                        UnitType = "ml"
                    },
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f9659b"),
                        Name = "item 3",
                        Quantity = 3000,
                        UnitType = "ml"
                    },
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f9659c"),
                        Name = "item 4",
                        Quantity = 4000,
                        UnitType = "ml"
                    }
                });

            var result = await this._sut.GetOpen();

            Assert.NotNull(result);
            Assert.Equal("599a98f185142b3ce0f965a0", result.Id);
            Assert.Equal("Test List", result.Name);
            Assert.Equal(false, result.IsDone);
            Assert.Equal(now.DateTime.Truncate(TimeSpan.FromSeconds(1)), result.CreatedOn.DateTime);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.OptionalItems.Count);

            Assert.NotNull(result.Items.Any(itm =>
                itm.IsDone &&
                itm.Amount == 10 &&
                itm.TotalAmount == 100 &&
                itm.Item.Id == "599a98f185142b3ce0f96598" &&
                itm.Item.Name == "item 1" &&
                itm.Item.Quantity == 1000 &&
                itm.Item.UnitType == "ml"));

            Assert.NotNull(result.Items.Any(itm =>
                itm.IsDone &&
                itm.Amount == 20 &&
                itm.TotalAmount == 200 &&
                itm.Item.Id == "599a98f185142b3ce0f96599" &&
                itm.Item.Name == "item 2" &&
                itm.Item.Quantity == 2000 &&
                itm.Item.UnitType == "ml"));

            Assert.NotNull(result.OptionalItems.Any(itm =>
                itm.IsDone &&
                itm.Amount == 30 &&
                itm.TotalAmount == 300 &&
                itm.Item.Id == "599a98f185142b3ce0f9659b" &&
                itm.Item.Name == "item 3" &&
                itm.Item.Quantity == 3000 &&
                itm.Item.UnitType == "ml"));

            Assert.NotNull(result.OptionalItems.Any(itm =>
                itm.IsDone &&
                itm.Amount == 40 &&
                itm.TotalAmount == 400 &&
                itm.Item.Id == "599a98f185142b3ce0f9659c" &&
                itm.Item.Name == "item 4" &&
                itm.Item.Quantity == 4000 &&
                itm.Item.UnitType == "ml"));
        }

        [Fact]
        public async Task DeleteNotFound()
        {
            this.ShoppingListRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((ShoppingList)null);
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
            this.ShoppingListRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync(new ShoppingList());
            this.ShoppingListRepositoryMock.Setup(x => x.Remove(It.IsAny<ObjectId>())).Returns(Task.CompletedTask);

            await this._sut.Delete("599a98f185142b3ce0f9659c");

            this.ShoppingListRepositoryMock.Verify(x => x.Remove(It.Is<ObjectId>(y => y.ToString() == "599a98f185142b3ce0f9659c")), Times.Once);
        }
    }
}
