using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KitchenServiceV2.Contract;
using KitchenServiceV2.Db.Mongo;
using KitchenServiceV2.Db.Mongo.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _config;
        private readonly IHttpClient _httpClient;

        public AccountController(IUserRepository repository, IConfiguration configuration, IHttpClient httpClient)
        {
            this._repository = repository;
            this._config = configuration;
            this._httpClient = httpClient;
        }

        [HttpPost("/api/account/login")]
        public async Task<AuthResponseDto> Login([FromBody] LoginDto value)
        {
            if (string.IsNullOrWhiteSpace(value.IdToken))
            {
                throw new ArgumentException("token cannot be empty");
            }

            var respose = await this._httpClient.GetAsync(string.Format("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}", value.IdToken));

            if ((int) respose.StatusCode != 200)
                throw new InvalidOperationException("Unable to verify google account token");
            var serialized = await respose.Content.ReadAsStringAsync();

            var user = JsonConvert.DeserializeObject<User>(serialized);
            var existingUser = await this._repository.FindByGoogleId(user.Sub);
            if (existingUser == null)
            {
                user.UserToken = Guid.NewGuid().ToString();
                await this._repository.Upsert(user);
            }
            else
            {
                user.UserToken = existingUser.UserToken;
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, user.UserToken)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                _config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(4),
                signingCredentials: creds);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "bearer"
            };
        }
    }
}
