using KitchenServiceV2.Db.Mongo;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class BaseDatabaseTests
    {
        protected string CollectionName;

        protected readonly IDbContext DbContext;
        protected readonly ITestOutputHelper Output;

        public BaseDatabaseTests(ITestOutputHelper output)
        {
            this.DbContext = new DbContext("mongodb://localhost:27017", "kitchenServiceV2Tests");
            this.Output = output;
        }

        public void Dispose()
        {
            this.DbContext.Db.DropCollection(this.CollectionName);
        }
    }
}
