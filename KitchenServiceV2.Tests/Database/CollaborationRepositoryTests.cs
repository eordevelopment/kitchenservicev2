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
    public class CollaborationRepositoryTests : BaseDatabaseTests
    {
        private readonly CollaborationRepository _sut;

        public CollaborationRepositoryTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new CollaborationRepository(this.DbContext);
            this.CollectionName = this._sut.CollectionName;
        }

        [Fact]
        public async Task CanFindPending()
        {
            await this._sut.Upsert(new List<Collaboration>
            {
                new Collaboration
                {
                    UserToken = "user1",
                    Collaborators = new List<Collaborator>()
                },
                new Collaboration
                {
                    UserToken = "user2",
                    Collaborators = new List<Collaborator>
                    {
                        new Collaborator
                        {
                            Email = "test@user.com",
                            AccessLevel = 1
                        }
                    }
                },
                new Collaboration
                {
                    UserToken = "user3",
                    Collaborators = new List<Collaborator>
                    {
                        new Collaborator
                        {
                            Email = "test@user.com",
                            UserId = new ObjectId("599a98f185142b3ce0f9659c"),
                            AccessLevel = 1
                        }
                    }
                }
            });

            var result = await this._sut.FindPending("test@user.com");
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal("user2", result.First().UserToken);
        }

        [Fact]
        public async Task CanUpdatePending()
        {
            await this._sut.Upsert(new List<Collaboration>
            {
                new Collaboration
                {
                    UserToken = "user2",
                    Collaborators = new List<Collaborator>
                    {
                        new Collaborator
                        {
                            Email = "test@user.com",
                            AccessLevel = 1
                        }
                    }
                }
            });

            var result = await this._sut.FindPending("test@user.com");
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal("user2", result.First().UserToken);

            result.First().Collaborators.First().UserId = new ObjectId("599a98f185142b3ce0f9659c");
            await this._sut.Upsert(result);

            var pending = await this._sut.FindPending("test@user.com");
            Assert.NotNull(pending);
            Assert.Equal(0, pending.Count);

            var all = await this._sut.GetAll("user2");
            Assert.NotNull(all);
            Assert.Equal(1, all.Count);
            Assert.Equal("user2", all.First().UserToken);
            Assert.Equal("599a98f185142b3ce0f9659c", all.First().Collaborators.First().UserId.ToString());

        }
    }
}
