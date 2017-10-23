using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        private readonly ICollaborationRepository _collaborationRepository;
        private readonly IConfiguration _config;
        private readonly IHttpClient _httpClient;

        public AccountController(IUserRepository repository, ICollaborationRepository collaborationRepository, IConfiguration configuration, IHttpClient httpClient)
        {
            this._repository = repository;
            this._collaborationRepository = collaborationRepository;
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

            var user = await this.GetUser(value.IdToken);
            await this.UpdateUserDetails(user);
            await this.UpdateCollaboration(user);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(this.GetToken(user)),
                TokenType = "bearer"
            };
        }

        [NonAction]
        private async Task<User> GetUser(string token)
        {
            var respose = await this._httpClient.GetAsync(string.Format("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}", token));

            if ((int)respose.StatusCode != 200)
                throw new InvalidOperationException("Unable to verify google account token");
            var serialized = await respose.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<User>(serialized);
        }

        private async Task UpdateCollaboration(User user)
        {
            var pendingCollaborations = await this._collaborationRepository.FindPending(user.Email);
            foreach (var collaboration in pendingCollaborations)
            {
                foreach (var collaborator in collaboration.Collaborators.Where(x => x.Email == user.Email))
                {
                    collaborator.UserToken = user.UserToken;
                }
            }

            if (pendingCollaborations.Any())
            {
                await this._collaborationRepository.Upsert(pendingCollaborations);
            }
        }

        [NonAction]
        private async Task UpdateUserDetails(User user)
        {
            var existingUser = await this._repository.FindByGoogleId(user.Sub);
            if (existingUser == null)
            {
                user.UserToken = Guid.NewGuid().ToString();
            }
            else
            {
                user.UserToken = existingUser.UserToken;
                user.Id = existingUser.Id;
            }
            await this._repository.Upsert(user);
        }

        [NonAction]
        private JwtSecurityToken GetToken(IDocument user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, user.UserToken)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(this._config["Tokens:Issuer"],
                this._config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
        }
    }
}
