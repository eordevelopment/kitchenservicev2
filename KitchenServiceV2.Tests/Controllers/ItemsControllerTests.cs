using KitchenServiceV2.Controllers;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Controllers
{
    public class ItemsControllerTests : BaseControllerTests
    {
        private readonly ItemsController _sut;

        public ItemsControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new ItemsController(this.ItemRepositoryMock.Object, this.ItemToBuyRepositoryMock.Object, this.RecipeRepositoryMock.Object);
            this.SetupController(this._sut);
        }
    }
}
