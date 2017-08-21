using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class AccountRepositoryTests : BaseDatabaseTests
    {
        private readonly IAccountRepository _sut;

        public AccountRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new AccountRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanFindByToken()
        {
            var account = new Account
            {
                HashedPassword = "HashPass",
                UserToken = "Token123",
                UserName = "testuser"
            };
            await this._sut.Insert(account);
            Assert.NotNull(account.Id);

            var dbAccount = await this._sut.FindByToken("Token123");
            Assert.NotNull(dbAccount);

            await this._sut.Remove(account);
            Assert.Null(await this._sut.Get(account.Id));
        }

        [Fact]
        public async Task CanFindByUserName()
        {
            var account = new Account
            {
                HashedPassword = "HashPass",
                UserToken = "Token123",
                UserName = "testuser"
            };
            await this._sut.Insert(account);
            Assert.NotNull(account.Id);

            var dbAccount = await this._sut.GetUser("TestUser");
            Assert.NotNull(dbAccount);

            await this._sut.Remove(account);
            Assert.Null(await this._sut.Get(account.Id));
        }

        [Fact]
        public async Task CanFindByUserNameAndPassword()
        {
            var account = new Account
            {
                HashedPassword = "HashPass",
                UserToken = "Token123",
                UserName = "testuser"
            };
            await this._sut.Insert(account);
            Assert.NotNull(account.Id);

            var dbAccount = await this._sut.GetUser("TestUser", "HashPass");
            Assert.NotNull(dbAccount);

            await this._sut.Remove(account);
            Assert.Null(await this._sut.Get(account.Id));
        }
    }
}
