using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Controllers;
using KitchenServiceV2.Db.Mongo.Schema;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Controllers
{
    public class AccountControllerTests : BaseControllerTests
    {
        private readonly AccountController _sut;
        
        private readonly Mock<IConfiguration> _configMock = new Mock<IConfiguration>(MockBehavior.Strict);
        private readonly Mock<IHttpClient> _httpMock = new Mock<IHttpClient>(MockBehavior.Strict);

        public AccountControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new AccountController(this.UserRepositoryMock.Object, this._configMock.Object, this._httpMock.Object);
            this._configMock.SetupGet(p => p["Tokens:Key"]).Returns("thisisnotthekeyyourarelookingfor");
            this._configMock.SetupGet(p => p["Tokens:Issuer"]).Returns("http://localhost:57236/");
        }

        [Fact]
        public async Task LoginInvalidParamShouldThrow()
        {
            var postData = new AccountDto();
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Login(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal("token cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task LoginInvalidTokenShouldThrow()
        {
            this._httpMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var postData = new AccountDto
            {
                IdToken = "SomeToken"
            };
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Login(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Code: BadRequest", exception.Message);
            }
        }

        [Fact]
        public async Task NewUserShouldSave()
        {
            var user = new User
            {
                Email = "test@user.com",
                Name = "Test User",
                Sub = "TestUser"
            };

            this._httpMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(user))
                });

            this.UserRepositoryMock.Setup(x => x.FindByGoogleId(It.IsAny<string>())).ReturnsAsync((User)null);
            this.UserRepositoryMock.Setup(x => x.Upsert(It.IsAny<User>())).Returns(Task.CompletedTask);

            var postData = new AccountDto
            {
                IdToken = "SomeToken"
            };

            var result = await this._sut.Login(postData);
            Assert.NotNull(result);

            var httpResponse = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(httpResponse);
            Assert.Equal(200, httpResponse.StatusCode);
            Assert.NotNull(httpResponse.Value);

            this.UserRepositoryMock.Verify(x => x.Upsert(It.Is<User>( u=> 
                u.Email == user.Email &&
                u.Name == user.Name &&
                u.Sub == user.Sub &&
                u.UserToken.Length > 1
            )), Times.Once);
        }

        [Fact]
        public async Task ExistingUserShouldNotSave()
        {
            var user = new User
            {
                Email = "test@user.com",
                Name = "Test User",
                Sub = "TestUser"
            };

            this._httpMock.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(user))
                });

            this.UserRepositoryMock.Setup(x => x.FindByGoogleId(It.IsAny<string>())).ReturnsAsync(new User
            {
                Email = user.Email,
                Id = new ObjectId("599a98f185142b3ce0f965a0"),
                UserToken = Guid.NewGuid().ToString(),
                Name = user.Name,
                Sub = user.Sub
            });

            var postData = new AccountDto
            {
                IdToken = "SomeToken"
            };

            var result = await this._sut.Login(postData);
            Assert.NotNull(result);

            var httpResponse = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(httpResponse);
            Assert.Equal(200, httpResponse.StatusCode);
            Assert.NotNull(httpResponse.Value);
        }
    }
}
