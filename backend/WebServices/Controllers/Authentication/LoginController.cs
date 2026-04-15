using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using WebServices.SharedBusiness;
using WebServices.DataAccess;

namespace WebServices.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private IConfiguration _config;
        private readonly DatabaseContext _context;

        public LoginController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginCreadentials loginCreadentials)
        {
            /// implementation with try-catch for error handling
            try
            {
                var inputBytes = Encoding.UTF8.GetBytes(loginCreadentials.Password);
                var hashBytes = SHA256.HashData(inputBytes);
                var hashPassword = Convert.ToHexString(hashBytes);
                loginCreadentials.Password = hashPassword;

                var authenticationProcess = new AuthenticationProcess(_context);

                var token = await authenticationProcess.Authenticate(
                loginCreadentials,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]
                );

                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { ex.Message });
            }
        }
    }
}
