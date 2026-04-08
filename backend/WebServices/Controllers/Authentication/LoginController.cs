using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Authentication;

namespace WebServices.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginCreadentials loginCreadentials)
        {
            /// implementation with try-catch for error handling
            try
            {
                var token = AuthenticationProcess.Authenticate(
                loginCreadentials,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]
                );

                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}
