using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo.Schema;
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

        protected bool CanView(IDocument document, IEnumerable<Collaboration> collaborations)
        {
            return document.UserToken == this.LoggedInUserToken ||
                   collaborations.Any(x => x.UserToken == document.UserToken && x.Collaborators.Any(y => y.UserToken == this.LoggedInUserToken));
        }

        protected bool CanEdit(IDocument document, IEnumerable<Collaboration> collaborations)
        {
            return document.UserToken == this.LoggedInUserToken ||
                   collaborations.Any(
                       x => x.UserToken == document.UserToken &&
                            x.Collaborators.Any(y => y.UserToken == this.LoggedInUserToken && y.AccessLevel == (int)AccessLevelEnum.Edit));
        }
    }
}
