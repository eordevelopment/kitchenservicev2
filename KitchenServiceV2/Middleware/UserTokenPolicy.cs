using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KitchenServiceV2.Middleware
{
    public class UserTokenPolicy : AuthorizationHandler<UserTokenPolicy>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserTokenPolicy requirement)
        {
            var mvcContext = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;
            if (mvcContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var headers = mvcContext?.HttpContext?.Request?.Headers;
            if (headers == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var token = headers["Authorization"];
            var rawVal = token.FirstOrDefault();
            var tokenVal = rawVal?.Replace("Basic ", "");

            if (tokenVal == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (this.IsValidToken(tokenVal))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            context.Fail();
            return Task.CompletedTask;
        }

        private bool IsValidToken(string token)
        {
            return true;
        }
    }
}
