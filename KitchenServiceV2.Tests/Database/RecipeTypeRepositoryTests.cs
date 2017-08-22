using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class RecipeTypeRepositoryTests : BaseDatabaseTests
    {
        private readonly IRecipeTypeRepository _sut;

        public RecipeTypeRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new RecipeTypeRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanFindByTokenAndName()
        {
            var recipeType = new RecipeType()
            {
                UserToken = "Token123",
                Name = "test item"
            };
            await this._sut.Upsert(recipeType);
            Assert.NotNull(recipeType.Id);

            var dbRecipeType = await this._sut.Find("Token123", "Test Item");
            Assert.NotNull(dbRecipeType);

            await this._sut.Remove(recipeType);
            Assert.Null(await this._sut.Get(recipeType.Id));
        }
    }
}
