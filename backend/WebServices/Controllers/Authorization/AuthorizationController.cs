using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Authorization
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        private IConfiguration _config;
        private readonly DatabaseContext _context;

        public AuthorizationController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAuthorizations()
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var authorizatioinProcess = new AuthenticationProcess(_context);
            var tokenValidation = authorizatioinProcess.TokenValidation(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);
            var validationResult = tokenValidation.Result;

            if (!validationResult.tokenIsValid)
            {
                return Unauthorized(new
                {
                    Message = validationResult.message
                });
            }

            return Ok(new
            {
                Message = "This is a protected resource. You are authorized to access it. User Role: " + validationResult.role
            });
        }
    }
}
