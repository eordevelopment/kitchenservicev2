using KitchenServiceV2.Controllers;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Controllers
{
    public class PlanControllerTests : BaseControllerTests
    {
        private readonly PlanController _sut;

        public PlanControllerTests(ITestOutputHelper output) : base(output)
        {
            this._sut = new PlanController(this.PlanRepositoryMock.Object, this.ItemRepositoryMock.Object, this.RecipeRepositoryMock.Object);
            this.SetupController(this._sut);
        }
    }
}
