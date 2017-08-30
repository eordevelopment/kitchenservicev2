using System;
using System.Collections.Generic;
using System.Linq;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests
{
    public class ShoppingListModelTests
    {
        private const string UserToken = "UserToken";

        private readonly IShoppingListModel _sut;
        private readonly ITestOutputHelper _output;

        public ShoppingListModelTests(ITestOutputHelper output)
        {
            this._sut = new ShoppingListModel();
            this._output = output;

            AutoMapperConfig.InitializeMapper();
        }

        [Fact]
        public void CreateShoppingListSingleRecipeAddsItemsToList()
        {
            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    Name = "test recipe 1",
                    UserToken = UserToken,
                    Key = "recipeKey",
                    RecipeItems = new List<RecipeItem>
                    {
                        new RecipeItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96598"),
                            Amount = 4
                        }
                    }
                }
            };

            var itemsById = new Dictionary<ObjectId, Item>
            {
                {
                    new ObjectId("599a98f185142b3ce0f96598"),
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "item 1",
                        Quantity = 1
                    }
                }
            };

            var result = this._sut.CreateShoppingList(UserToken, recipes, itemsById);

            Assert.NotNull(result);
            Assert.Equal(false, result.IsDone);
            Assert.Equal(UserToken, result.UserToken);
            Assert.Equal(DateTime.Now.ToString("ddd, MMM-dd yyyy"), result.Name);
            Assert.Equal(1, result.Items.Count);
            Assert.False(result.OptionalItems.Any());

            Assert.True(result.Items.Any(itm => 
                itm.IsDone == false &&
                itm.ItemId.ToString() == "599a98f185142b3ce0f96598" &&
                itm.Amount == 3 && itm.TotalAmount == 4
            ));
        }

        [Fact]
        public void CreateShoppingListMultipleRecipeSameItemAddsItemsToList()
        {
            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    Name = "test recipe 1",
                    UserToken = UserToken,
                    Key = "recipeKey",
                    RecipeItems = new List<RecipeItem>
                    {
                        new RecipeItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96598"),
                            Amount = 4
                        }
                    }
                },
                new Recipe
                {
                    Id = new ObjectId("599a98f185142b3ce0f96599"),
                    Name = "test recipe 2",
                    UserToken = UserToken,
                    Key = "recipeKey2",
                    RecipeItems = new List<RecipeItem>
                    {
                        new RecipeItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96598"),
                            Amount = 2
                        }
                    }
                }
            };

            var itemsById = new Dictionary<ObjectId, Item>
            {
                {
                    new ObjectId("599a98f185142b3ce0f96598"),
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "item 1",
                        Quantity = 1
                    }
                }
            };

            var result = this._sut.CreateShoppingList(UserToken, recipes, itemsById);

            Assert.NotNull(result);
            Assert.Equal(1, result.Items.Count);
            Assert.False(result.OptionalItems.Any());

            Assert.True(result.Items.Any(itm =>
                itm.IsDone == false &&
                itm.ItemId.ToString() == "599a98f185142b3ce0f96598" &&
                itm.Amount == 5 && itm.TotalAmount == 6
            ));
        }

        [Fact]
        public void CreateShoppingListSingleRecipeAddsOptionalItemsToList()
        {
            var recipes = new List<Recipe>
            {
                new Recipe
                {
                    Id = new ObjectId("599a98f185142b3ce0f965a0"),
                    Name = "test recipe 1",
                    UserToken = UserToken,
                    Key = "recipeKey",
                    RecipeItems = new List<RecipeItem>
                    {
                        new RecipeItem
                        {
                            ItemId = new ObjectId("599a98f185142b3ce0f96598"),
                            Amount = 4
                        }
                    }
                }
            };

            var itemsById = new Dictionary<ObjectId, Item>
            {
                {
                    new ObjectId("599a98f185142b3ce0f96598"),
                    new Item
                    {
                        Id = new ObjectId("599a98f185142b3ce0f96598"),
                        Name = "item 1",
                        Quantity = 10
                    }
                }
            };

            var result = this._sut.CreateShoppingList(UserToken, recipes, itemsById);

            Assert.NotNull(result);
            Assert.Equal(false, result.IsDone);
            Assert.Equal(UserToken, result.UserToken);
            Assert.Equal(DateTime.Now.ToString("ddd, MMM-dd yyyy"), result.Name);
            Assert.Equal(1, result.OptionalItems.Count);
            Assert.False(result.Items.Any());

            Assert.True(result.OptionalItems.Any(itm =>
                itm.IsDone == false &&
                itm.ItemId.ToString() == "599a98f185142b3ce0f96598" &&
                itm.Amount == 4 && itm.TotalAmount == 4
            ));
        }
    }
}
