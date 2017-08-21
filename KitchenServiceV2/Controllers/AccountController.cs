using System;
using System.Threading.Tasks;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using Microsoft.AspNetCore.Mvc;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IAccountRepository _repository;

        public AccountController(IAccountRepository repository)
        {
            this._repository = repository;
        }

        [HttpPost("/api/account/register")]
        public async Task<string> Register([FromBody]AccountDto value)
        {
            if (string.IsNullOrWhiteSpace(value.UserName) || string.IsNullOrWhiteSpace(value.HashedPassword))
            {
                throw new ArgumentException("Username and/or password cannot be empty");
            }

            var existingAccount = await this._repository.GetUser(value.UserName);
            if (existingAccount != null)
            {
                throw new InvalidOperationException("User already exists.");
            }

            var account = new Account
            {
                HashedPassword = value.HashedPassword,
                UserName = value.UserName.ToLower(),
                UserToken = Guid.NewGuid().ToString()
            };
            await this._repository.Insert(account);

            return account.UserToken;
        }

        [HttpPost("/api/account/login")]
        public async Task<string> Login([FromBody]AccountDto value)
        {
            if (string.IsNullOrWhiteSpace(value.UserName) || string.IsNullOrWhiteSpace(value.HashedPassword))
            {
                throw new ArgumentException("Username and/or password cannot be empty");
            }

            var account = await this._repository.GetUser(value.UserName, value.HashedPassword);

            if (account == null) throw new InvalidOperationException("Username and/or password is incorrect");

            return account.UserToken;
        }
    }
}
