using KitchenServiceV2.Controllers;
using KitchenServiceV2.Db.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit.Abstractions;

namespace KitchenServiceV2.Tests.Controllers
{
    public class BaseControllerTests
    {
        protected readonly Mock<IAccountRepository> AccountRepositoryMock = new Mock<IAccountRepository>(MockBehavior.Strict);
        protected readonly Mock<ICategoryRepository> CategoriyRepositoryMock = new Mock<ICategoryRepository>(MockBehavior.Strict);
        protected readonly Mock<IItemRepository> ItemRepositoryMock = new Mock<IItemRepository>(MockBehavior.Strict);
        protected readonly Mock<IRecipeTypeRepository> RecipeTypeRepositoryMock = new Mock<IRecipeTypeRepository>(MockBehavior.Strict);
        protected readonly Mock<IRecipeRepository> RecipeRepositoryMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
        protected readonly Mock<IPlanRepository> PlanRepositoryMock = new Mock<IPlanRepository>(MockBehavior.Strict);

        protected ITestOutputHelper Output;

        public BaseControllerTests(ITestOutputHelper output)
        {
            this.Output = output;
            AutoMapperConfig.InitializeMapper();
        }
        protected void SetupController(BaseController controller)
        {
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Basic UserToken";
        }
    }
}
