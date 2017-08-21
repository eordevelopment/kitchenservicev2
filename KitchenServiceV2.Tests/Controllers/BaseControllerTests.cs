using KitchenServiceV2.Controllers;
using KitchenServiceV2.Db.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KitchenServiceV2.Tests.Controllers
{
    public class BaseControllerTests
    {
        protected readonly Mock<IAccountRepository> AccountRepositoryMock = new Mock<IAccountRepository>(MockBehavior.Strict);
        protected readonly Mock<ICategoryRepository> CategoriyRepositoryMock = new Mock<ICategoryRepository>(MockBehavior.Strict);
        protected readonly Mock<IItemRepository> ItemRepositoryMock = new Mock<IItemRepository>(MockBehavior.Strict);

        public BaseControllerTests()
        {
            Startup.InitializeMapper();
        }
        protected void SetupController(BaseController controller)
        {
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Basic UserToken";
        }
    }
}
