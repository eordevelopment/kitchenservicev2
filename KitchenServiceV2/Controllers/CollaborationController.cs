using KitchenServiceV2.Db.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CollaborationController : BaseController
    {
        private readonly ICollaborationRepository _collaborationRepository;
        private readonly IUserRepository _userRepository;

        public CollaborationController(
            ICollaborationRepository collaborationRepository,
            IUserRepository userRepository)
        {
            this._collaborationRepository = collaborationRepository;
            this._userRepository = userRepository;
        }
    }
}
