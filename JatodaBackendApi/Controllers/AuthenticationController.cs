using JatodaBackendApi.Model;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BCryptNet = BCrypt.Net.BCrypt;

namespace JatodaBackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ITokenService _tokenService;
        private readonly IUserProvider<User> _userProvider;

        public AuthenticationController(IUserProvider<User> userProvider, ITokenService tokenService,
            ILogger<AuthenticationController> logger)
        {
            _userProvider = userProvider;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest? model)
        {
            if (model == null)
            {
                return BadRequest("Invalid payload.");
            }

            var user = await _userProvider.GetByUsernameAsync(model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Passwordhash))
            {
                return Unauthorized();
            }

            var token = _tokenService.GenerateToken(user.Id.ToString(), user.Username);
            return Ok(new
            {
                Token = token
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest? model)
        {
            if (model == null)
            {
                return BadRequest("Invalid payload.");
            }

            if (await _userProvider.GetByUsernameAsync(model.Username) != null)
            {
                return BadRequest("Username is already taken");
            }

            // Check also for email

            
            var passwordHash = BCryptNet.HashPassword(model.Password);
            var user = new User
            {
                Username = model.Username,
                Passwordhash = passwordHash,
                Email = model.Email,
                Passwordsalt = "test"
            };

            await _userProvider.AddUserAsync(user);
            return Ok();
        }
    }
}