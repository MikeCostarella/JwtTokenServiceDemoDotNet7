using JwtTokenService.DataAccess;
using JwtTokenService.DataAccess.Models;
using JwtTokenServiceDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtTokenServiceDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Member Variables

        private readonly CosmosDBContext context;
        private readonly IConfiguration configuration;

        #endregion Member Variables

        #region Constructors

        public AuthController(IConfiguration configuration, CosmosDBContext context) {
            this.configuration = configuration;
            this.context = context;
        }

        #endregion Constructors

        #region Actions

        [HttpPost("register")]
        public ActionResult<User> Register(UserDto request)
        {
            var user = context.Users.FirstOrDefault(x => x.Username.ToLower().Equals(request.Username.ToLower().Trim()));
            if (user != null)
            {
                return BadRequest("User already exists.");
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };
            context.Users.Add(user);
            context.SaveChanges();
            return Ok(user);
        }

        [HttpPost("login")]
        public ActionResult<User> Login(UserDto request)
        {
            var user = context.Users.FirstOrDefault(x => x.Username.ToLower().Equals(request.Username.ToLower().Trim()));
            if (user == null)
            {
                return BadRequest("User not found");
            }
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password");
            }
            var token = CreateToken(user);
            return Ok(token);
        }

        #endregion Actions

        #region Utilities

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };
            var secretToken = configuration.GetSection("AppSettings:Token");
            var encoding = Encoding.UTF8.GetBytes(secretToken.Value!);
            var key = new SymmetricSecurityKey(encoding);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var jwtToken = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: credentials);
            var jwt = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return jwt;
        }

        #endregion Utilities
    }
}
