using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class UserRepositoryTests : BaseDatabaseTests
    {
        private readonly IUserRepository _sut;

        public UserRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new UserRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanFindByGoogleId()
        {
            var account = new User
            {
                UserToken = "Token123",
                Email = "test@user.com",
                Name = "Test User",
                Sub = "testUser"
            };
            await this._sut.Upsert(account);
            Assert.NotNull(account.Id);

            var dbUser = await this._sut.FindByGoogleId("testUser");
            Assert.NotNull(dbUser);

            await this._sut.Remove(dbUser);
            Assert.Null(await this._sut.Get(dbUser.Id));
        }
    }
}
