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
    public class CategoryControllerTests : BaseControllerTests
    {
        private readonly CategoryController _sut;
        

        public CategoryControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new CategoryController(this.CategoriyRepositoryMock.Object, this.ItemRepositoryMock.Object);
            this.SetupController(this._sut);         
        }

        [Fact]
        public async Task GetNoCategoriesReturnsEmpty()
        {
            this.CategoriyRepositoryMock.Setup(x => x.GetAll(It.IsAny<string>())).ReturnsAsync((List<Category>)null);

            var result = await this._sut.Get();

            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task GetReturnsCategories()
        {
            this.CategoriyRepositoryMock.Setup(x => x.GetAll(It.IsAny<string>()))
                .ReturnsAsync(new List<Category>
                {
                    new Category
                    {
                        Id = new ObjectId("599a98f185142b3ce0f965a0"),
                        Name = "category 1",
                        UserToken = "UserToken",
                        ItemIds = new List<ObjectId>
                        {
                            new ObjectId("599a98f185142b3ce0f96599"),
                            new ObjectId("599a98f185142b3ce0f9659b"),
                            new ObjectId("599a98f185142b3ce0f9659c")
                        }
                    }
                });

            var result = await this._sut.Get();

            Assert.NotNull(result);
            Assert.Equal(1, result.Count);

            var category = result.FirstOrDefault();
            Assert.NotNull(category);
            Assert.Equal("599a98f185142b3ce0f965a0", category.Id);
            Assert.Equal("category 1", category.Name);
            Assert.NotNull(category.Items);
            Assert.False(category.Items.Any());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetCategoryNoIdShouldThrow(string id)
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
        public async Task GetCategoryNotFoundShouldThrow()
        {
            this.CategoriyRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((Category)null);

            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Get("599a98f185142b3ce0f9659c"));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal("No resource with id: 599a98f185142b3ce0f9659c", exception.Message);
            }
        }

        [Fact]
        public async Task GetCategoryReturnsCategory()
        {
            this.CategoriyRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new Category
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    Name = "category 1",
                    UserToken = "UserToken",
                    ItemIds = new List<ObjectId>
                    {
                        new ObjectId("599a98f185142b3ce0f96599"),
                        new ObjectId("599a98f185142b3ce0f9659b"),
                        new ObjectId("599a98f185142b3ce0f9659c")
                    }
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
                    },
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f9659c"),
                        Name = "item 3",
                        Quantity = 3,
                        UnitType = "3"
                    }
                });

            var category = await this._sut.Get("599a98f185142b3ce0f965a0");
            Assert.NotNull(category);
            Assert.Equal("599a98f185142b3ce0f965a0", category.Id);
            Assert.Equal("category 1", category.Name);
            Assert.NotNull(category.Items);
            Assert.Equal(3, category.Items.Count);

            var item1 = category.Items.FirstOrDefault();
            Assert.NotNull(item1);
            Assert.Equal("599a98f185142b3ce0f96599", item1.Id);
            Assert.Equal("item 1", item1.Name);
            Assert.Equal(1, item1.Quantity);
            Assert.Equal("1", item1.UnitType);
        }

        [Fact]
        public async Task DeleteNotFound()
        {
            this.CategoriyRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((Category)null);
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
            this.CategoriyRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync(new Category
            {
                UserToken = "UserToken"
            });
            this.CategoriyRepositoryMock.Setup(x => x.Remove(It.IsAny<ObjectId>())).Returns(Task.CompletedTask);

            await this._sut.Delete("599a98f185142b3ce0f9659c");

            this.CategoriyRepositoryMock.Verify(x => x.Remove(It.Is<ObjectId>(y => y.ToString() == "599a98f185142b3ce0f9659c")), Times.Once);
        }

        [Fact]
        public async Task PutNotFound()
        {
            this.CategoriyRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((Category)null);
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("599a98f185142b3ce0f9659c", new CategoryDto
            {
                Name = "test category"
            }));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal("No resource with id: 599a98f185142b3ce0f9659c", exception.Message);
            }
        }

        [Fact]
        public async Task PutEmptyNameShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("id", new CategoryDto()));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PutEmptyItemShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("id", new CategoryDto
            {
                Name = "Test Item",
                Items = new List<ItemDto>
                {
                    new ItemDto()
                }
            }));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Item Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostEmptyNameShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(new CategoryDto()));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostEmptyItemShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(new CategoryDto
            {
                Name = "Test Item",
                Items = new List<ItemDto>
                {
                    new ItemDto()
                }
            }));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Item Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostAlreadyExists()
        {
            this.CategoriyRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Category());

            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(
                new CategoryDto
                {
                    Name = "test category"
                }));

            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Category already exists.", exception.Message);
            }
        }

        [Fact]
        public async Task PostCorrectlySaves()
        {
            this.CategoriyRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Category)null);

            this.CategoriyRepositoryMock.Setup(x => x.Upsert(It.IsAny<Category>())).Returns(Task.CompletedTask);
            this.ItemRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Item>>())).Returns(Task.CompletedTask);

            var category = new CategoryDto
            {
                Name = "Test Category",
                Items = new List<ItemDto>
                {
                    new ItemDto
                    {
                        Name = "New Item 1",
                        Quantity = 1,
                        UnitType = "1"
                    },
                    new ItemDto
                    {
                        Name = "New Item 2",
                        Quantity = 2,
                        UnitType = "2"
                    },
                    new ItemDto
                    {
                        Name = "Existing Item 1",
                        Quantity = 1,
                        UnitType = "1",
                        Id = "599a98f185142b3ce0f9659c"
                    },
                    new ItemDto
                    {
                        Name = "Existing Item 2",
                        Quantity = 2,
                        UnitType = "2",
                        Id = "599a98f185142b3ce0f9659d"
                    }
                }
            };
            

            await this._sut.Post(category);
            this.CategoriyRepositoryMock
                .Verify(x => x.Upsert(It.Is<Category>(cat =>
                    cat.Name == "test category" &&
                    cat.UserToken == "UserToken" &&
                    cat.ItemIds.Count == 4 &&
                    cat.ItemIds.Count(i => i.ToString() == "599a98f185142b3ce0f9659c") == 1 &&
                    cat.ItemIds.Count(i => i.ToString() == "599a98f185142b3ce0f9659d") == 1
                )), Times.Once);

            this.ItemRepositoryMock
                .Verify(x => x.Upsert(It.Is<IReadOnlyCollection<Item>>(y =>
                    y.Count == 4 &&
                    y.Any(itm => itm.Name == "new item 1" && itm.UserToken == "UserToken" && itm.Quantity == 1 && itm.UnitType == "1") &&
                    y.Any(itm => itm.Name == "new item 2" && itm.UserToken == "UserToken" && itm.Quantity == 2 && itm.UnitType == "2") &&
                    y.Any(itm => itm.Name == "existing item 1" && itm.Id.ToString() == "599a98f185142b3ce0f9659c") &&
                    y.Any(itm => itm.Name == "existing item 2" && itm.Id.ToString() == "599a98f185142b3ce0f9659d")
                )), Times.Once);
        }

        [Fact]
        public async Task PutShouldSave()
        {
            this.CategoriyRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new Category
                {
                    UserToken = "UserToken"
                });

            this.CategoriyRepositoryMock.Setup(x => x.Upsert(It.IsAny<Category>())).Returns(Task.CompletedTask);

            await this._sut.Put("599a98f185142b3ce0f96598", new CategoryDto
            {
                Id = "599a98f185142b3ce0f96598",
                Name = "Test Type"
            });


            this.CategoriyRepositoryMock
                .Verify(x => x.Upsert(It.Is<Category>(rt =>
                    rt.Name == "test type" &&
                    rt.Id.ToString() == "599a98f185142b3ce0f96598" &&
                    rt.UserToken == "UserToken"
                )), Times.Once);
        }
    }
}
