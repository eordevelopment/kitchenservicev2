using System;
using System.Threading.Tasks;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Controllers;
using KitchenServiceV2.Db.Mongo.Schema;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Controllers
{
    public class AccountControllerTests : BaseControllerTests
    {
        private readonly AccountController _sut;
        

        public AccountControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new AccountController(this.AccountRepositoryMock.Object);
        }

        [Fact]
        public async Task RegisterInvalidParamShouldThrow()
        {
            var postData = new AccountDto();
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Register(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal("Username and/or password cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task RegisterUserAlreadyExistsShouldThrow()
        {
            this.AccountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(new Account());

            var postData = new AccountDto
            {
                HashedPassword = "password",
                UserName = "UserName"
            };
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Register(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("User already exists.", exception.Message);
            }
        }

        [Fact]
        public async Task RegisterShouldSave()
        {
            this.AccountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync((Account)null);
            this.AccountRepositoryMock.Setup(x => x.Upsert(It.IsAny<Account>())).Returns(Task.CompletedTask);

            var postData = new AccountDto
            {
                HashedPassword = "password",
                UserName = "UserName"
            };

            var result = await this._sut.Register(postData);

            Assert.NotNull(result);

            // verify that UserName is converted to lower case
            this.AccountRepositoryMock
                .Verify(x => x.Upsert(It.Is<Account>(y => y.HashedPassword == "password" && y.UserName == "username")), Times.Once);
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
                Assert.Equal("Username and/or password cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task LoginNoAccountShouldThrow()
        {
            this.AccountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Account)null);

            var postData = new AccountDto
            {
                HashedPassword = "password",
                UserName = "UserName"
            };
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Login(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Username and/or password is incorrect", exception.Message);
            }
        }

        [Fact]
        public async Task LoginValidAccount()
        {
            this.AccountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Account
                {
                    UserToken = Guid.NewGuid().ToString()
                });

            var postData = new AccountDto
            {
                HashedPassword = "password",
                UserName = "UserName"
            };
            var result = await this._sut.Login(postData);

            Assert.NotNull(result);
        }
    }
}
