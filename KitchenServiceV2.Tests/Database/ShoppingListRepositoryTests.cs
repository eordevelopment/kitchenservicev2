using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class ShoppingListRepositoryTests : BaseDatabaseTests
    {
        private readonly IShoppingListRepository _sut;

        public ShoppingListRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new ShoppingListRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task GetOpenShouldReturnOnlyOpen()
        {
            var lists = new List<ShoppingList>
            {
                new ShoppingList
                {
                    CreatedOnUnixSeconds = DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds(),
                    Name = "closed 1",
                    IsDone = true,
                    UserToken = "UserToken"
                },
                new ShoppingList
                {
                    CreatedOnUnixSeconds = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = "open",
                    IsDone = false,
                    UserToken = "UserToken"
                },
                new ShoppingList
                {
                    CreatedOnUnixSeconds = DateTimeOffset.Now.AddDays(-2).ToUnixTimeSeconds(),
                    Name = "closed 2",
                    IsDone = true,
                    UserToken = "UserToken"
                }
            };

            await this._sut.Upsert(lists);

            var result = await this._sut.GetOpen("UserToken");
            Assert.NotNull(result);
            Assert.Equal("open", result.Name);
            Assert.Equal(false, result.IsDone);
        }

        [Fact]
        public async Task GetClosedShouldReturnOnlyClosedInOrder()
        {
            var lists = new List<ShoppingList>
            {
                new ShoppingList
                {
                    CreatedOnUnixSeconds = DateTimeOffset.Now.AddDays(-5).ToUnixTimeSeconds(),
                    Name = "closed 1",
                    IsDone = true,
                    UserToken = "UserToken"
                },
                new ShoppingList
                {
                    CreatedOnUnixSeconds = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = "open",
                    IsDone = false,
                    UserToken = "UserToken"
                },
                new ShoppingList
                {
                    CreatedOnUnixSeconds = DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds(),
                    Name = "closed 2",
                    IsDone = true,
                    UserToken = "UserToken"
                },
                new ShoppingList
                {
                    CreatedOnUnixSeconds = DateTimeOffset.Now.AddDays(-2).ToUnixTimeSeconds(),
                    Name = "closed 3",
                    IsDone = true,
                    UserToken = "UserToken"
                }
            };

            await this._sut.Upsert(lists);

            var result = await this._sut.GetClosed("UserToken", 0, 10);
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.False(result.Any(x => !x.IsDone));
            Assert.True(result[0].CreatedOnUnixSeconds > result[1].CreatedOnUnixSeconds);
            Assert.True(result[1].CreatedOnUnixSeconds > result[2].CreatedOnUnixSeconds);
        }
    }
}
