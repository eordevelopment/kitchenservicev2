using System.Collections.Generic;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using Xunit;
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

        [Fact]
        public async Task CanFindByTokenAndName()
        {
            var item = new Item
            {
                Id = ObjectId.Empty,
                UserToken = "Token123",
                Name = "test item",
                Quantity = 10,
                UnitType = "ml"
            };
            await this._sut.Insert(item);
            Assert.NotNull(item.Id);

            var dbItem = await this._sut.FindItem("Token123", "Test Item");
            Assert.NotNull(dbItem);

            await this._sut.Remove(item);
            Assert.Null(await this._sut.Get(item.Id));
        }

        [Fact]
        public async Task CanSearchFullName()
        {
            var item = new Item
            {
                UserToken = "Token123",
                Name = "test item",
                Quantity = 10,
                UnitType = "ml"
            };
            await this._sut.Insert(item);
            Assert.NotNull(item.Id);

            var dbItems = await this._sut.SearchItems("Token123", "test item", 10);
            Assert.NotNull(dbItems);
            Assert.True(dbItems.Count >= 1);
        }

        [Fact]
        public async Task CanSearchStartsWith()
        {
            var item = new Item
            {
                UserToken = "Token123",
                Name = "test item",
                Quantity = 10,
                UnitType = "ml"
            };
            await this._sut.Insert(item);
            Assert.NotNull(item.Id);

            var dbItems = await this._sut.SearchItems("Token123", "test", 10);
            Assert.NotNull(dbItems);
            Assert.True(dbItems.Count >= 1);
        }

        [Fact]
        public async Task CanSearchLimit()
        {
            for (int i = 0; i < 20; i++)
            {
                var item = new Item
                {
                    UserToken = "Token123",
                    Name = "test item " + i,
                    Quantity = 10,
                    UnitType = "ml"
                };
                await this._sut.Insert(item);
                Assert.NotNull(item.Id);
            }

            var dbItems = await this._sut.SearchItems("Token123", "test", 10);
            Assert.NotNull(dbItems);
            Assert.True(dbItems.Count == 10);
        }

        [Fact]
        public async Task CanBulkInsert()
        {
            var items = new List<Item>
            {
                new Item
                {
                    Id = ObjectId.Empty,
                    UserToken = "Token123",
                    Name = "test item 1",
                    Quantity = 10,
                    UnitType = "ml"
                },
                new Item
                {
                    Id = ObjectId.Empty,
                    UserToken = "Token123",
                    Name = "test item 2",
                    Quantity = 20,
                    UnitType = "ml"
                }
            };

            await this._sut.Insert(items);
            foreach (var item in items)
            {
                Assert.NotEqual(ObjectId.Empty, item.Id);
            }
        }
    }
}
