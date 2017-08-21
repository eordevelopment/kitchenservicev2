using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class CategoryRepositoryTests : BaseDatabaseTests
    {
        private readonly ICategoryRepository _sut;

        public CategoryRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new CategoryRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanUpdate()
        {
            var category = new Category
            {
                Name = "test category",
                UserToken = "UserToken"
            };
            await this._sut.Insert(category);
            Assert.NotNull(category.Id);
            this.Output.WriteLine(category.Id.ToString());

            category.Name = "updated";
            await this._sut.Update(category);

            var dbCategory = await this._sut.Find("UserToken", "Updated");
            Assert.NotNull(dbCategory);
            Assert.Equal(category.Id.ToString(), dbCategory.Id.ToString());

            await this._sut.Remove(category);
            Assert.Null(await this._sut.Get(category.Id));
        }

        [Fact]
        public async Task CanSaveItemIds()
        {
            var category = new Category
            {
                Name = "test category",
                UserToken = "UserToken"
            };
            category.ItemIds.Add(new ObjectId());
            await this._sut.Insert(category);
            Assert.NotNull(category.Id);
            this.Output.WriteLine(category.Id.ToString());

            var dbCategory = await this._sut.Find("UserToken", "test category");
            Assert.NotNull(dbCategory);
            Assert.Equal(category.Id.ToString(), dbCategory.Id.ToString());
            Assert.NotNull(dbCategory.ItemIds);
            Assert.Equal(1, dbCategory.ItemIds.Count);

            await this._sut.Remove(category.Id);
            Assert.Null(await this._sut.Get(category.Id));
        }

        [Fact]
        public async Task CanGetAll()
        {
            for (var i = 0; i < 5; i++)
            {
                var category = new Category
                {
                    Name = "category " + i,
                    UserToken = "UserToken"
                };
                await this._sut.Insert(category);
            }

            var dbCategories = (await this._sut.GetAll("UserToken")).ToList();
            Assert.True(dbCategories.Count >= 5, "dbCategories.Count >= 5");
        }
    }
}
