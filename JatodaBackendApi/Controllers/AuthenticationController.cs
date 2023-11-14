using JatodaBackendApi.Model;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        public AuthenticationController(
            IUserProvider<User> userProvider,
            ITokenService tokenService,
            ILogger<AuthenticationController> logger
        )
        {
            _userProvider = userProvider;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest? model)
        {
            if (
                model == null
                || string.IsNullOrEmpty(model.Username)
                || string.IsNullOrEmpty(model.Password)
            )
            {
                return BadRequest("Invalid payload. Username and Password should not be empty.");
            }

            var user = await _userProvider.GetByUsernameAsync(model.Username);
            if (user == null || !BCryptNet.Verify(model.Password, user.Passwordhash))
            {
                return BadRequest("Invalid credentials. Please check your username and password.");
            }

            var token = _tokenService.GenerateToken(user.Id.ToString(), user.Username);
            Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });

            return Ok(
                new
                {
                    message = "Login successful.",
                    username = user.Username,
                    token
                }
            );
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            var token = HttpContext.Request.Headers["Authorization"].ToString().Split()[1];
            _tokenService.RevokeToken(token);
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest? model)
        {
            if (
                model == null
                || string.IsNullOrWhiteSpace(model.Username)
                || string.IsNullOrWhiteSpace(model.Email)
                || string.IsNullOrWhiteSpace(model.Password)
            )
            {
                return BadRequest("Invalid payload.");
            }

            if (await _userProvider.GetByUsernameAsync(model.Username) != null)
            {
                return BadRequest("Username is already taken");
            }

            if (await _userProvider.GetByEmailAsync(model.Email) != null)
            {
                return BadRequest("Email is already in use");
            }

            var passwordHash = BCryptNet.HashPassword(model.Password);
            var user = new User
            {
                Username = model.Username,
                Passwordhash = passwordHash,
                Email = model.Email,
                Passwordsalt = "test" // TODO: remove salt from model and from here
            };

            var createdUser = await _userProvider.AddUserAsync(user);
            return CreatedAtAction(
                nameof(AuthenticationController.Register),
                new { id = user.Id },
                createdUser
            );
        }

        [HttpOptions] // Handle OPTIONS request
        public IActionResult Options()
        {
            // Set the necessary CORS headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            // Return a 204 No Content response
            return NoContent();
        }
    }
}
