using System.Linq;
using System.Threading.Tasks;
using KitchenServiceV2.Db.Mongo;
using Microsoft.AspNetCore.Authorization;

namespace KitchenServiceV2.Middleware
{
    public class UserTokenPolicy : AuthorizationHandler<UserTokenPolicyRequirement>
    {
        private readonly IAccountRepository _accountRepository;
        public UserTokenPolicy(IAccountRepository accountRepository)
        {
            this._accountRepository = accountRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserTokenPolicyRequirement requirement)
        {
            var mvcContext = context.Resource as Microsoft.AspNetCore.Http.DefaultHttpContext;
            if (mvcContext == null)
            {
                context.Fail();
            }

            var headers = mvcContext?.Request?.Headers;
            if (headers == null)
            {
                context.Fail();
            }
            else
            {
                var token = headers["Authorization"];
                var rawVal = token.FirstOrDefault();
                var tokenVal = rawVal?.Replace("Basic ", "");

                if (tokenVal == null)
                {
                    context.Fail();
                }

                if (await this.IsValidToken(tokenVal))
                {
                    context.Succeed(requirement);
                }

                context.Fail();
            }
        }

        private async Task<bool> IsValidToken(string token)
        {
            var account = await this._accountRepository.FindByToken(token);
            return account != null;
        }
    }
}
