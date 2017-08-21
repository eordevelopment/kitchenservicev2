using System;
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

                var token = Request.Headers["Authorization"];
                var rawVal = token.FirstOrDefault();
                var tokenVal = rawVal?.Replace("Basic ", "");

                if(string.IsNullOrWhiteSpace(tokenVal)) throw new UnauthorizedAccessException();

                this._userToken = tokenVal;
                return this._userToken;
            }
        }
    }
}
