using System;
using System.Collections.Generic;
using System.Linq;
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
            this._sut = new AccountController(
                this.UserRepositoryMock.Object, 
                this.CollaborationRepositoryMock.Object,
                this._configMock.Object, 
                this._httpMock.Object);

            this._configMock.SetupGet(p => p["Tokens:Key"]).Returns("thisisnotthekeyyourarelookingfor");
            this._configMock.SetupGet(p => p["Tokens:Issuer"]).Returns("http://localhost:57236/");
        }

        [Fact]
        public async Task LoginInvalidParamShouldThrow()
        {
            var postData = new LoginDto();
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

            var postData = new LoginDto
            {
                IdToken = "SomeToken"
            };
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Login(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Unable to verify google account token", exception.Message);
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
            this.CollaborationRepositoryMock.Setup(x => x.FindPending(It.IsAny<string>())).ReturnsAsync(new List<Collaboration>());

            var postData = new LoginDto
            {
                IdToken = "SomeToken"
            };

            var result = await this._sut.Login(postData);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.TokenType);

            this.UserRepositoryMock.Verify(x => x.Upsert(It.Is<User>( u=> 
                u.Email == user.Email &&
                u.Name == user.Name &&
                u.Sub == user.Sub &&
                u.UserToken.Length > 1
            )), Times.Once);
        }

        [Fact]
        public async Task ExistingUserShouldUpdate()
        {
            var user = new User
            {
                Email = "test@user.com",
                Name = "Test User",
                Sub = "TestUser",
                UserToken = Guid.NewGuid().ToString()
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
                UserToken = user.UserToken,
                Name = user.Name,
                Sub = user.Sub
            });
            this.UserRepositoryMock.Setup(x => x.Upsert(It.IsAny<User>())).Returns(Task.CompletedTask);
            this.CollaborationRepositoryMock.Setup(x => x.FindPending(It.IsAny<string>())).ReturnsAsync(new List<Collaboration>());

            var postData = new LoginDto
            {
                IdToken = "SomeToken"
            };

            var result = await this._sut.Login(postData);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.TokenType);

            this.UserRepositoryMock.Verify(x => x.Upsert(It.Is<User>(u =>
                u.Email == user.Email &&
                u.Name == user.Name &&
                u.Sub == user.Sub &&
                u.UserToken.Equals(user.UserToken) &&
                u.Id.ToString().Equals("599a98f185142b3ce0f965a0")
            )), Times.Once);
        }

        [Fact]
        public async Task ExistingUserShouldUpdateCollaboration()
        {
            var user = new User
            {
                Email = "test@user.com",
                Name = "Test User",
                Sub = "TestUser",
                UserToken = Guid.NewGuid().ToString()
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
                UserToken = user.UserToken,
                Name = user.Name,
                Sub = user.Sub
            });
            this.UserRepositoryMock.Setup(x => x.Upsert(It.IsAny<User>())).Returns(Task.CompletedTask);
            this.CollaborationRepositoryMock.Setup(x => x.Upsert(It.IsAny<IReadOnlyCollection<Collaboration>>())).Returns(Task.CompletedTask);
            this.CollaborationRepositoryMock.Setup(x => x.FindPending(It.IsAny<string>())).ReturnsAsync(new List<Collaboration>
            {
                new Collaboration
                {
                    UserToken = "someOtherUser",
                    Id = new ObjectId("599a98f185142b3ce0f9659b"),
                    Collaborators = new List<Collaborator>
                    {
                        new Collaborator
                        {
                            Email = "test@user.com",
                            AccessLevel = 0
                        }
                    }
                }
            });

            var postData = new LoginDto
            {
                IdToken = "SomeToken"
            };

            var result = await this._sut.Login(postData);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.TokenType);

            this.CollaborationRepositoryMock.Verify(x => x.Upsert(It.IsAny<IReadOnlyCollection<Collaboration>>()), Times.Once);

            this.CollaborationRepositoryMock.Verify(x => x.Upsert(It.Is<IReadOnlyCollection<Collaboration>>(u =>
                u.Count == 1 &&
                u.First().Collaborators.Count == 1 &&
                u.FirstOrDefault(c =>
                    c.Collaborators.Count == 1 &&
                    c.Collaborators.FirstOrDefault(cl =>
                        cl.Email.Equals("test@user.com") &&
                        cl.UserId.ToString().Equals("599a98f185142b3ce0f965a0")) != null
                ) != null
            )), Times.Once);
        }
    }
}
