using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo.Repository;
using KitchenServiceV2.Db.Mongo.Schema;
using MongoDB.Bson;
using Xunit;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Database
{
    public class ItemToBuyRepositoryTests : BaseDatabaseTests
    {
        private readonly ItemToBuyRepository _sut;

        public ItemToBuyRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new ItemToBuyRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanFindByItem()
        {
            var itemToBuy = new ItemToBuy
            {
                UserToken = "userToken",
                ItemId = new ObjectId("599a98f185142b3ce0f96599")
            };
            await this._sut.Upsert(itemToBuy);
            Assert.NotNull(itemToBuy.Id);

            var dbItem = await this._sut.FindByItemId(new ObjectId("599a98f185142b3ce0f96599"));
            Assert.NotNull(dbItem);

            await this._sut.Remove(dbItem);
            Assert.Null(await this._sut.Get(dbItem.Id));
        }

        [Fact]
        public async Task CanDeleteMany()
        {
            var itemsToBuy = new List<ItemToBuy>
            {
                new ItemToBuy
                {
                    UserToken = "userToken",
                    ItemId = new ObjectId("599a98f185142b3ce0f96599")
                },
                new ItemToBuy
                {
                    UserToken = "userToken",
                    ItemId = new ObjectId("599a98f185142b3ce0f96599")
                },
                new ItemToBuy
                {
                    UserToken = "userToken",
                    ItemId = new ObjectId("599a98f185142b3ce0f96599")
                }
            };

            await this._sut.Upsert(itemsToBuy);
            var existingItems = await this._sut.GetAll("userToken");
            Assert.Equal(3, existingItems.Count);

            await this._sut.Remove("userToken");
            existingItems = await this._sut.GetAll("userToken");
            Assert.Equal(0, existingItems.Count);
        }

        [Fact]
        public async Task CanFindByItemIds()
        {
            var itemsToBuy = new List<ItemToBuy>
            {
                new ItemToBuy
                {
                    UserToken = "userToken",
                    ItemId = new ObjectId("599a98f185142b3ce0f96597")
                },
                new ItemToBuy
                {
                    UserToken = "userToken",
                    ItemId = new ObjectId("599a98f185142b3ce0f96598")
                },
                new ItemToBuy
                {
                    UserToken = "userToken",
                    ItemId = new ObjectId("599a98f185142b3ce0f96599")
                }
            };

            await this._sut.Upsert(itemsToBuy);
            var existingItems = await this._sut.GetAll("userToken");
            Assert.Equal(3, existingItems.Count);

            existingItems = await this._sut.FindByItemIds("userToken", new[] { new ObjectId("599a98f185142b3ce0f96597"), new ObjectId("599a98f185142b3ce0f96598") });
            Assert.Equal(2, existingItems.Count);
            Assert.NotNull(existingItems.FirstOrDefault(x => x.ItemId.ToString() == "599a98f185142b3ce0f96597"));
            Assert.NotNull(existingItems.FirstOrDefault(x => x.ItemId.ToString() == "599a98f185142b3ce0f96598"));
        }
    }
}
