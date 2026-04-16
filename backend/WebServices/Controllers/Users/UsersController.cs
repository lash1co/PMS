using Domain.SharedConstants;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServices.DataAccess;
using WebServices.Repositories;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Users
{
    /// <summary>
    /// Represents an API controller that manages user-related operations for the application.
    /// </summary>
    /// <remarks>The UsersController provides endpoints for creating and managing users. All routes are
    /// prefixed with 'api/users'. This controller requires dependency injection of configuration and database context
    /// objects. Authorization is enforced on relevant endpoints to ensure only permitted users can perform certain
    /// actions.</remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private IConfiguration _config;
        private readonly DatabaseContext _context;

        public UsersController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        /// <summary>
        /// |
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
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

            if (validationResult.role != UserConstants.RoleConstants.AdminRole)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    validationResult.role + " does not have permission to execute this process."
                );
            }

            var userProcess = new UsersRepository(_context);
            var createProcess = userProcess.CreateUser(user);
            var processResult = createProcess.Result;

            if (!processResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    processResult.Message
                );
            }

            return Ok(processResult);
        }
    }
}
