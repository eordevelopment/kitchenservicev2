using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class ItemRepositoryTests : BaseDatabaseTests
    {
        private readonly IItemRepository _sut;

        public ItemRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new ItemRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }
    }
}
