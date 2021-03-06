﻿using System;
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
    public class ListControllerTests : BaseControllerTests
    {
        private readonly ListController _sut;

        private readonly Mock<IShoppingListModel> _shoppingListModelMock = new Mock<IShoppingListModel>(MockBehavior.Strict);

        public ListControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new ListController(
                this.PlanRepositoryMock.Object, 
                this.ItemRepositoryMock.Object, 
                this.ItemToBuyRepositoryMock.Object,
                this.ShoppingListRepositoryMock.Object, 
                this.RecipeRepositoryMock.Object,
                this._shoppingListModelMock.Object);
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
                            TotalAmount = 100,
                            RecipeIds = new HashSet<ObjectId>{ new ObjectId("599a98f185142b3ce0f96590"), new ObjectId("599a98f185142b3ce0f96591") }
                        },
                        new ShoppingListItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96599"),
                            IsDone = false,
                            Amount = 20,
                            TotalAmount = 200,
                            RecipeIds = new HashSet<ObjectId>{ new ObjectId("599a98f185142b3ce0f96590"), new ObjectId("599a98f185142b3ce0f96591") }
                        }
                    },
                    OptionalItems = new List<ShoppingListItem>
                    {
                        new ShoppingListItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f9659b"),
                            IsDone = false,
                            Amount = 30,
                            TotalAmount = 300,
                            RecipeIds = new HashSet<ObjectId>{ new ObjectId("599a98f185142b3ce0f96591") }
                        },
                        new ShoppingListItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f9659c"),
                            IsDone = false,
                            Amount = 40,
                            TotalAmount = 500,
                            RecipeIds = new HashSet<ObjectId>{ new ObjectId("599a98f185142b3ce0f96590") }
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

            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>
                {
                    new Recipe
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96590"),
                        Name = "recipe 1"
                    },
                    new Recipe
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96591"),
                        Name = "recipe 2"
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

            Assert.NotNull(result.Items.FirstOrDefault(itm =>
                itm.IsDone &&
                itm.Amount == 10 &&
                itm.TotalAmount == 100 &&
                itm.Item.Id == "599a98f185142b3ce0f96598" &&
                itm.Item.Name == "item 1" &&
                itm.Item.Quantity == 1000 &&
                itm.Item.UnitType == "ml" &&
                itm.Recipes?.Count() == 2 &&
                itm.Recipes?.FirstOrDefault(r => r.Id == "599a98f185142b3ce0f96590" && r.Name == "recipe 1") != null &&
                itm.Recipes?.FirstOrDefault(r => r.Id == "599a98f185142b3ce0f96591" && r.Name == "recipe 2") != null));

            Assert.NotNull(result.Items.FirstOrDefault(itm =>
                !itm.IsDone &&
                itm.Amount == 20 &&
                itm.TotalAmount == 200 &&
                itm.Item.Id == "599a98f185142b3ce0f96599" &&
                itm.Item.Name == "item 2" &&
                itm.Item.Quantity == 2000 &&
                itm.Item.UnitType == "ml" &&
                itm.Recipes != null &&
                itm.Recipes.Count() == 2 &&
                itm.Recipes?.FirstOrDefault(r => r.Id == "599a98f185142b3ce0f96590" && r.Name == "recipe 1") != null &&
                itm.Recipes?.FirstOrDefault(r => r.Id == "599a98f185142b3ce0f96591" && r.Name == "recipe 2") != null));

            Assert.NotNull(result.OptionalItems.FirstOrDefault(itm =>
                !itm.IsDone &&
                itm.Amount == 30 &&
                itm.TotalAmount == 300 &&
                itm.Item.Id == "599a98f185142b3ce0f9659b" &&
                itm.Item.Name == "item 3" &&
                itm.Item.Quantity == 3000 &&
                itm.Item.UnitType == "ml" &&
                itm.Recipes != null &&
                itm.Recipes.Count() == 1 &&
                itm.Recipes.FirstOrDefault(r => r.Id == "599a98f185142b3ce0f96591" && r.Name == "recipe 2") != null));

            Assert.NotNull(result.OptionalItems.FirstOrDefault(itm =>
                !itm.IsDone &&
                itm.Amount == 40 &&
                itm.TotalAmount == 500 &&
                itm.Item.Id == "599a98f185142b3ce0f9659c" &&
                itm.Item.Name == "item 4" &&
                itm.Item.Quantity == 4000 &&
                itm.Item.UnitType == "ml" &&
                itm.Recipes != null &&
                itm.Recipes.Count() == 1 &&
                itm.Recipes.FirstOrDefault(r => r.Id == "599a98f185142b3ce0f96590" && r.Name == "recipe 1") != null));
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
            this.ShoppingListRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync(new ShoppingList
            {
                UserToken = "UserToken"
            });
            this.ShoppingListRepositoryMock.Setup(x => x.Remove(It.IsAny<ObjectId>())).Returns(Task.CompletedTask);

            await this._sut.Delete("599a98f185142b3ce0f9659c");

            this.ShoppingListRepositoryMock.Verify(x => x.Remove(It.Is<ObjectId>(y => y.ToString() == "599a98f185142b3ce0f9659c")), Times.Once);
        }

        [Fact]
        public async Task PutCorrectlyMaps()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new ShoppingListDto
            {
                Name = "test list",
                Id = "599a98f185142b3ce0f965a0",
                CreatedOn = now,
                IsDone = false,
                Items = new List<ShoppingListItemDto>
                {
                    new ShoppingListItemDto
                    {
                        IsDone = true,
                        Amount = 2,
                        TotalAmount = 4,
                        Item = new ItemDto{ Id = "599a98f185142b3ce0f96597" }
                    }
                },
                OptionalItems = new List<ShoppingListItemDto>
                {
                    new ShoppingListItemDto
                    {
                        IsDone = false,
                        Amount = 10,
                        TotalAmount = 10,
                        Item = new ItemDto{ Id = "599a98f185142b3ce0f96598" },
                        Recipes = new List<RecipeDto>
                        {
                            new RecipeDto
                            {
                                Id = "599a98f185142b3ce0f96590",
                                Name = "recipe 1"
                            }
                        }
                    }
                }
            };

            this.ShoppingListRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new ShoppingList
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    UserToken = "UserToken",
                    Items = new List<ShoppingListItem>()
                });

            this.ShoppingListRepositoryMock.Setup(x => x.Upsert(It.IsAny<ShoppingList>()))
                .Returns(Task.CompletedTask);

            this.ItemRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Item>
                {
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "item 1",
                        Quantity = 10,
                        UserToken = "UserToken"
                    }
                });
            this.ItemRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Item>>()))
                .Returns(Task.CompletedTask);

            var result = await this._sut.Put("599a98f185142b3ce0f965a0", dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.OptionalItems.Count);

            var item = result.OptionalItems.FirstOrDefault(x => x.Item.Name == "item 1");
            Assert.NotNull(item);
            Assert.Equal(1, item.Recipes?.Count());
            Assert.NotNull(item.Recipes?.FirstOrDefault(x => x.Id == "599a98f185142b3ce0f96590" && x.Name == "recipe 1"));

            this.ShoppingListRepositoryMock.Verify(x => x.Upsert(It.Is<ShoppingList>(l =>
                l.IsDone &&
                l.Name == "test list" &&
                l.Id.ToString() == "599a98f185142b3ce0f965a0" &&
                l.UserToken == "UserToken" &&
                l.CreatedOnUnixSeconds == now.ToUnixTimeSeconds() &&
                l.Items.Count == 1 && l.OptionalItems.Count == 1 &&
                l.Items.Any(i =>
                    i.IsDone &&
                    i.Amount == 2 &&
                    i.TotalAmount == 4
                ) &&
                l.OptionalItems.Any(i =>
                    !i.IsDone &&
                    i.Amount == 10 &&
                    i.TotalAmount == 10 &&
                    i.ItemId.ToString() == "599a98f185142b3ce0f96598" &&
                    i.RecipeIds.Count == 1 &&
                    i.RecipeIds.FirstOrDefault(rId => rId.ToString() == "599a98f185142b3ce0f96590") != null
                )
            )), Times.Once);
        }

        [Fact]
        public async Task PutShouldIncrementStock()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new ShoppingListDto
            {
                Name = "test list",
                Id = "599a98f185142b3ce0f965a0",
                CreatedOn = now,
                IsDone = false,
                Items = new List<ShoppingListItemDto>
                {
                    new ShoppingListItemDto
                    {
                        IsDone = true,
                        Amount = 2,
                        TotalAmount = 4,
                        Item = new ItemDto{ Id = "599a98f185142b3ce0f96598" }
                    }
                }
            };

            this.ShoppingListRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new ShoppingList
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    UserToken = "UserToken",
                    Items = new List<ShoppingListItem>()
                });

            this.ItemRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Item>
                {
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "item 1",
                        Quantity = 10,
                        UserToken = "UserToken"
                    }
                });

            this.ShoppingListRepositoryMock.Setup(x => x.Upsert(It.IsAny<ShoppingList>()))
                .Returns(Task.CompletedTask);

            this.ItemRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Item>>()))
                .Returns(Task.CompletedTask);

            var result = await this._sut.Put("599a98f185142b3ce0f965a0", dto);

            Assert.NotNull(result);

            this.ItemRepositoryMock.Verify(x => x.Upsert(It.Is<IReadOnlyCollection<Item>>(i => 
                i.Count == 1 &&
                i.Any(itm => itm.Id.ToString() == "599a98f185142b3ce0f96598" && itm.Quantity == 12)
            )), Times.Once);
        }

        [Fact]
        public async Task PutShouldDecrementStock()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new ShoppingListDto
            {
                Name = "test list",
                Id = "599a98f185142b3ce0f965a0",
                CreatedOn = now,
                IsDone = false,
                Items = new List<ShoppingListItemDto>
                {
                    new ShoppingListItemDto
                    {
                        IsDone = false,
                        Amount = 2,
                        TotalAmount = 4,
                        Item = new ItemDto{ Id = "599a98f185142b3ce0f96598" }
                    }
                }
            };

            this.ShoppingListRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new ShoppingList
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    UserToken = "UserToken",
                    Items = new List<ShoppingListItem>
                    {
                        new ShoppingListItem
                        {
                            IsDone = true,
                            ItemId = new ObjectId("599a98f185142b3ce0f96598"),
                            Amount = 2,
                            TotalAmount = 4
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
                        Quantity = 10,
                        UserToken = "UserToken"
                    }
                });

            this.ShoppingListRepositoryMock.Setup(x => x.Upsert(It.IsAny<ShoppingList>()))
                .Returns(Task.CompletedTask);

            this.ItemRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Item>>()))
                .Returns(Task.CompletedTask);

            var result = await this._sut.Put("599a98f185142b3ce0f965a0", dto);

            Assert.NotNull(result);

            this.ItemRepositoryMock.Verify(x => x.Upsert(It.Is<IReadOnlyCollection<Item>>(i =>
                i.Count == 1 &&
                i.Any(itm => itm.Id.ToString() == "599a98f185142b3ce0f96598" && itm.Quantity == 8)
            )), Times.Once);
        }

        [Fact]
        public async Task PutShouldReturnUpdatedStock()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new ShoppingListDto
            {
                Name = "test list",
                Id = "599a98f185142b3ce0f965a0",
                CreatedOn = now,
                IsDone = false,
                Items = new List<ShoppingListItemDto>
                {
                    new ShoppingListItemDto
                    {
                        IsDone = true,
                        Amount = 2,
                        TotalAmount = 4,
                        Item = new ItemDto{ Id = "599a98f185142b3ce0f96598" }
                    }
                }
            };

            this.ShoppingListRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new ShoppingList
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    UserToken = "UserToken",
                    Items = new List<ShoppingListItem>()
                });

            this.ItemRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Item>
                {
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "item 1",
                        Quantity = 10,
                        UserToken = "UserToken"
                    }
                });

            this.ShoppingListRepositoryMock.Setup(x => x.Upsert(It.IsAny<ShoppingList>()))
                .Returns(Task.CompletedTask);

            this.ItemRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Item>>()))
                .Returns(Task.CompletedTask);

            var result = await this._sut.Put("599a98f185142b3ce0f965a0", dto);

            Assert.NotNull(result);
            Assert.Equal(12, result.Items.First().Item.Quantity);
        }

        [Fact]
        public async Task GenerateShouldPreserveDuplicateRecipeFromMultiplePlans()
        {
            this.ShoppingListRepositoryMock.Setup(x => x.GetOpen(It.IsAny<string>()))
                .ReturnsAsync((ShoppingList)null);

            this.PlanRepositoryMock.Setup(x => x.GetOpen(It.IsAny<string>()))
                .ReturnsAsync(new List<Plan>
                {
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        UserToken = "UserToken",
                        IsDone = false,
                        DateTimeUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        PlanItems = new List<PlanItem>
                        {
                            new PlanItem
                            {
                                IsDone = false,
                                RecipeId = new ObjectId("599a98f185142b3ce0f96598")
                            }
                        }
                    },
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f9659e"),
                        UserToken = "UserToken",
                        IsDone = false,
                        DateTimeUnixSeconds = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                        PlanItems = new List<PlanItem>
                        {
                            new PlanItem
                            {
                                IsDone = false,
                                RecipeId = new ObjectId("599a98f185142b3ce0f96598")
                            }
                        }
                    }
                });

            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>
                {
                    new Recipe
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        UserToken = "UserToken",
                        Name = "recipe",
                        Key = "key",
                        RecipeItems = new List<RecipeItem>
                        {
                            new RecipeItem
                            {
                                ItemId = new ObjectId("599a98f185142b3ce0f96599"),
                                Amount = 1
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
                        UserToken = "UserToken",
                        Name = "item",
                        Quantity = 2,
                        UnitType = "ml"
                    }
                });

            this._shoppingListModelMock.Setup(x => x.CreateShoppingList(It.IsAny<string>(), It.IsAny<ICollection<Recipe>>(), It.IsAny<IReadOnlyDictionary<ObjectId, Item>>(), It.IsAny<IReadOnlyCollection<ItemToBuy>>()))
                .Returns(new ShoppingList());

            this.ShoppingListRepositoryMock.Setup(x => x.Upsert(It.IsAny<ShoppingList>())).Returns(Task.CompletedTask);

            this.ItemToBuyRepositoryMock.Setup(x => x.GetAll(It.IsAny<String>())).ReturnsAsync(new List<ItemToBuy>());
            this.ItemToBuyRepositoryMock.Setup(x => x.Remove(It.IsAny<String>())).Returns(Task.CompletedTask);

            await this._sut.GenerateList();

            this._shoppingListModelMock
                .Verify(x => x.CreateShoppingList(
                        It.IsAny<string>(),
                        It.Is<ICollection<Recipe>>(col =>
                            col.Count(r => r.Id.ToString() == "599a98f185142b3ce0f96598") == 2
                        ),
                        It.IsAny<IReadOnlyDictionary<ObjectId, Item>>(),
                        It.IsAny<IReadOnlyCollection<ItemToBuy>>()),

                    Times.Once);
        }

        [Fact]
        public async Task GenerateShouldPreserveDuplicateRecipeFromPlan()
        {
            this.ShoppingListRepositoryMock.Setup(x => x.GetOpen(It.IsAny<string>()))
                .ReturnsAsync((ShoppingList)null);

            this.PlanRepositoryMock.Setup(x => x.GetOpen(It.IsAny<string>()))
                .ReturnsAsync(new List<Plan>
                {
                    new Plan
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        UserToken = "UserToken",
                        IsDone = false,
                        DateTimeUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        PlanItems = new List<PlanItem>
                        {
                            new PlanItem
                            {
                                IsDone = false,
                                RecipeId = new ObjectId("599a98f185142b3ce0f96598")
                            },
                            new PlanItem
                            {
                                IsDone = false,
                                RecipeId = new ObjectId("599a98f185142b3ce0f96598")
                            }
                        }
                    }
                });

            this.RecipeRepositoryMock.Setup(x => x.Get(It.IsAny<IReadOnlyCollection<ObjectId>>()))
                .ReturnsAsync(new List<Recipe>
                {
                    new Recipe
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        UserToken = "UserToken",
                        Name = "recipe",
                        Key = "key",
                        RecipeItems = new List<RecipeItem>
                        {
                            new RecipeItem
                            {
                                ItemId = new ObjectId("599a98f185142b3ce0f96599"),
                                Amount = 1
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
                        UserToken = "UserToken",
                        Name = "item",
                        Quantity = 2,
                        UnitType = "ml"
                    }
                });

            this._shoppingListModelMock.Setup(x => x.CreateShoppingList(It.IsAny<string>(), It.IsAny<ICollection<Recipe>>(), It.IsAny<IReadOnlyDictionary<ObjectId, Item>>(), It.IsAny<IReadOnlyCollection<ItemToBuy>>()))
                .Returns(new ShoppingList());

            this.ItemToBuyRepositoryMock.Setup(x => x.GetAll(It.IsAny<String>())).ReturnsAsync(new List<ItemToBuy>());
            this.ItemToBuyRepositoryMock.Setup(x => x.Remove(It.IsAny<String>())).Returns(Task.CompletedTask);

            this.ShoppingListRepositoryMock.Setup(x => x.Upsert(It.IsAny<ShoppingList>())).Returns(Task.CompletedTask);

            await this._sut.GenerateList();

            this._shoppingListModelMock
                .Verify(x => x.CreateShoppingList(
                    It.IsAny<string>(), 
                    It.Is<ICollection<Recipe>>(col =>
                        col.Count(r => r.Id.ToString() == "599a98f185142b3ce0f96598") == 2
                    ), 
                    It.IsAny<IReadOnlyDictionary<ObjectId, Item>>(),
                        It.IsAny<IReadOnlyCollection<ItemToBuy>>()), 
                    Times.Once);
        }
    }
}
