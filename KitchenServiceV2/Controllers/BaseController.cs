using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace KitchenServiceV2.Controllers
{
    public class BaseController : Controller
    {
        private string _userToken;
        protected string LoggedInUserToken
        {
            get
            {
                if (this._userToken != null) return this._userToken;

                var claims = User?.Claims;
                var userId = claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                this._userToken = userId;
                return this._userToken;
            }
        }
    }
}
