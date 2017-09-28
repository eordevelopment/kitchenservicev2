using System.Collections.Generic;
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
    public class RecipeRepositoryTests : BaseDatabaseTests
    {
        private readonly IRecipeRepository _sut;

        public RecipeRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new RecipeRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanFindByTokenAndName()
        {
            var recipe = new Recipe
            {
                UserToken = "Token123",
                Name = "test item"
            };
            await this._sut.Upsert(recipe);
            Assert.NotNull(recipe.Id);

            var dbRecipe = await this._sut.Find("Token123", "Test Item");
            Assert.NotNull(dbRecipe);

            await this._sut.Remove(recipe);
            Assert.Null(await this._sut.Get(recipe.Id));
        }

        [Fact]
        public async Task CanFindByKey()
        {
            var recipe = new Recipe
            {
                UserToken = "Token123",
                Name = "test item",
                Key = "recipeKey",
                RecipeItems = new List<RecipeItem>
                {
                    new RecipeItem
                    {
                        Amount = 10,
                        Instructions = "instruction",
                        ItemId = new ObjectId()
                    }
                },
                RecipeSteps = new List<RecipeStep>
                {
                    new RecipeStep
                    {
                        Description = "step 1",
                        StepNumber = 1
                    }
                }
            };
            await this._sut.Upsert(recipe);
            Assert.NotNull(recipe.Id);

            var dbRecipe = await this._sut.Find("recipeKey");
            Assert.NotNull(dbRecipe);
            Assert.True(dbRecipe.RecipeItems.Count == 1);
            Assert.True(dbRecipe.RecipeSteps.Count == 1);

            await this._sut.Remove(recipe);
            Assert.Null(await this._sut.Get(recipe.Id));
        }

        [Fact]
        public async Task CanFindByItem()
        {
            var recipe = new Recipe
            {
                UserToken = "Token123",
                Name = "test item",
                Key = "recipeKey",
                RecipeItems = new List<RecipeItem>
                {
                    new RecipeItem
                    {
                        Amount = 10,
                        Instructions = "instruction",
                        ItemId = new ObjectId("599a98f185142b3ce0f965a0")
                    }
                },
                RecipeSteps = new List<RecipeStep>
                {
                    new RecipeStep
                    {
                        Description = "step 1",
                        StepNumber = 1
                    }
                }
            };
            await this._sut.Upsert(recipe);
            Assert.NotNull(recipe.Id);

            var dbRecipes = await this._sut.FindByItem("Token123", new ObjectId("599a98f185142b3ce0f965a0"));
            Assert.NotNull(dbRecipes);
            Assert.Equal(1, dbRecipes.Count);

            var dbRecipe = dbRecipes.First();
            Assert.True(dbRecipe.RecipeItems.Count == 1);
            Assert.True(dbRecipe.RecipeSteps.Count == 1);

            await this._sut.Remove(recipe);
            Assert.Null(await this._sut.Get(recipe.Id));
        }
    }
}
