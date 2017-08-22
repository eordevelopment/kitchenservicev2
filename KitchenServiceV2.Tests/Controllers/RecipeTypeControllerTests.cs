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
    public class RecipeTypeControllerTests : BaseControllerTests
    {
        private readonly RecipeTypeController _sut;

        public RecipeTypeControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new RecipeTypeController(this.RecipeTypeRepositoryMock.Object);
            this.SetupController(this._sut);
        }

        [Fact]
        public async Task GetNoItemsReturnsEmpty()
        {
            this.RecipeTypeRepositoryMock.Setup(x => x.GetAll(It.IsAny<string>())).ReturnsAsync((List<RecipeType>)null);

            var result = await this._sut.Get();

            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task GetShouldMap()
        {
            this.RecipeTypeRepositoryMock.Setup(x => x.GetAll(It.IsAny<string>()))
                .ReturnsAsync(new List<RecipeType>
                {
                    new RecipeType{ Id = new ObjectId("599a98f185142b3ce0f965a0"), Name = "type1", UserToken = "UserToken" },
                    new RecipeType{ Id = new ObjectId("599a98f185142b3ce0f96598"), Name = "type2", UserToken = "UserToken" }
                });

            var result = await this._sut.Get();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var item1 = result.FirstOrDefault();
            Assert.NotNull(item1);
            Assert.Equal("599a98f185142b3ce0f965a0", item1.Id);
            Assert.Equal("type1", item1.Name);
        }

        [Fact]
        public async Task DeleteNotFound()
        {
            this.RecipeTypeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((RecipeType)null);
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
            this.RecipeTypeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync(new RecipeType());
            this.RecipeTypeRepositoryMock.Setup(x => x.Remove(It.IsAny<ObjectId>())).Returns(Task.FromResult(true));

            await this._sut.Delete("599a98f185142b3ce0f9659c");

            this.RecipeTypeRepositoryMock.Verify(x => x.Remove(It.Is<ObjectId>(y => y.ToString() == "599a98f185142b3ce0f9659c")), Times.Once);
        }

        [Fact]
        public async Task PutNotFound()
        {
            this.RecipeTypeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>())).ReturnsAsync((RecipeType)null);
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("599a98f185142b3ce0f9659c", new RecipeTypeDto
            {
                Name = "test type"
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
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Put("id", new RecipeTypeDto()));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostEmptyNameShouldThrow()
        {
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(new RecipeTypeDto()));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Name cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task PostAlreadyExists()
        {
            this.RecipeTypeRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RecipeType());

            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Post(
                new RecipeTypeDto
                {
                    Name = "test type"
                }));

            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Recipe Type already exists.", exception.Message);
            }
        }

        [Fact]
        public async Task PostShouldSave()
        {
            this.RecipeTypeRepositoryMock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((RecipeType)null);

            this.RecipeTypeRepositoryMock.Setup(x => x.Insert(It.IsAny<RecipeType>())).Returns(Task.FromResult(true));

            var result = await this._sut.Post(new RecipeTypeDto
            {
                Name = "Test Type"
            });

            Assert.NotNull(result);

            this.RecipeTypeRepositoryMock.Verify(x => x.Insert(It.Is<RecipeType>(rt => rt.Name == "test type")), Times.Once);
        }

        [Fact]
        public async Task PutShouldSave()
        {
            this.RecipeTypeRepositoryMock.Setup(x => x.Get(It.IsAny<ObjectId>()))
                .ReturnsAsync(new RecipeType());

            this.RecipeTypeRepositoryMock.Setup(x => x.Update(It.IsAny<RecipeType>())).Returns(Task.FromResult(true));

            var result = await this._sut.Put("599a98f185142b3ce0f96598", new RecipeTypeDto
            {
                Id = "599a98f185142b3ce0f96598",
                Name = "Test Type"
            });

            Assert.NotNull(result);
            Assert.NotEqual(ObjectId.Empty.ToString(), result);

            this.RecipeTypeRepositoryMock
                .Verify(x => x.Update(It.Is<RecipeType>(rt =>
                    rt.Name == "test type" &&
                    rt.Id.ToString() == "599a98f185142b3ce0f96598" &&
                    rt.UserToken == "UserToken"
                )), Times.Once);
        }
    }
}
