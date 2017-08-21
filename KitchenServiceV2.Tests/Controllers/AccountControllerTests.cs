using System;
using System.Threading.Tasks;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Controllers;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly AccountController _sut;
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new Mock<IAccountRepository>(MockBehavior.Strict);

        public AccountControllerTests(ITestOutputHelper output)
        {
            this._sut = new AccountController(_accountRepositoryMock.Object);
        }

        [Fact]
        public async Task RegisterInvalidParamShouldThrow()
        {
            var postData = new AccountDto();
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Register(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Username and/or password cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task RegisterUserAlreadyExistsShouldThrow()
        {
            this._accountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(new Account());

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
            this._accountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync((Account)null);
            this._accountRepositoryMock.Setup(x => x.Insert(It.IsAny<Account>())).Returns(Task.FromResult(true));

            var postData = new AccountDto
            {
                HashedPassword = "password",
                UserName = "UserName"
            };

            var result = await this._sut.Register(postData);

            Assert.NotNull(result);

            // verify that UserName is converted to lower case
            this._accountRepositoryMock
                .Verify(x => x.Insert(It.Is<Account>(y => y.HashedPassword == "password" && y.UserName == "username")), Times.Once);
        }

        [Fact]
        public async Task LoginInvalidParamShouldThrow()
        {
            var postData = new AccountDto();
            var exceptionAsync = Record.ExceptionAsync(() => this._sut.Login(postData));
            if (exceptionAsync != null)
            {
                var exception = await exceptionAsync;
                Assert.IsType(typeof(InvalidOperationException), exception);
                Assert.Equal("Username and/or password cannot be empty", exception.Message);
            }
        }

        [Fact]
        public async Task LoginNoAccountShouldThrow()
        {
            this._accountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Account)null);

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
            this._accountRepositoryMock.Setup(x => x.GetUser(It.IsAny<string>(), It.IsAny<string>()))
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
