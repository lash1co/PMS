using Domain.SharedConstants;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServices.DataAccess;
using WebServices.Repositories;
using WebServices.SharedBusiness;
using Microsoft.EntityFrameworkCore;

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
        /// Retrieves all users from the data store.
        /// </summary>
        /// <remarks>This method requires authentication. The response will be empty if there are no users
        /// in the data store.</remarks>
        /// <returns>An <see cref="ActionResult{T}"/> containing a collection of all users. Returns a 204 No Content response if
        /// no users are found; otherwise, returns a 200 OK response with the list of users.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var validation = new TokenRoleValidator(_context);

            var validationResult = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

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

            var users = await _context.Users
                .ToListAsync();

            if (users.Count == 0)
            {
                return NoContent();
            }

            return Ok(users);
        }

        /// <summary>
        /// Retrieves the user with the specified unique identifier.
        /// </summary>
        /// <remarks>This method requires authentication. Returns HTTP 200 (OK) with the user if found, or
        /// HTTP 204 (No Content) if no user exists with the specified identifier.</remarks>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing the user if found; otherwise, a response with status code 204
        /// (No Content).</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User?>> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u  => u.Id == id)
                .FirstOrDefaultAsync();

            if (user is null)
            {
                return NoContent();
            }

            return Ok(user);
        }

        /// <summary>
        /// Creates a new user account based on the provided user information.
        /// </summary>
        /// <remarks>This action requires a valid JWT authorization token with administrative privileges.
        /// Only users with the Admin role are permitted to create new user accounts.</remarks>
        /// <param name="user">The user details to create. The user object must contain all required fields for user creation.</param>
        /// <returns>An IActionResult indicating the result of the operation. Returns 200 OK with the created user information if
        /// successful; 401 Unauthorized if the authorization token is missing or invalid; 403 Forbidden if the caller
        /// does not have administrative permissions; or 500 Internal Server Error if user creation fails.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var validation = new TokenRoleValidator(_context);

            var validationResult = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

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

        /// <summary>
        /// Updates the details of an existing user. Requires administrative privileges.
        /// </summary>
        /// <remarks>This action requires a valid JWT authorization token with an administrator role. Only
        /// users with administrative privileges can perform this operation.</remarks>
        /// <param name="user">The user entity containing the updated information. All required fields must be provided. Cannot be null.</param>
        /// <returns>An IActionResult indicating the result of the update operation. Returns 200 (OK) with the update result if
        /// successful; 401 (Unauthorized) if the authorization token is missing or invalid; 403 (Forbidden) if the
        /// caller does not have administrative permissions; or 500 (Internal Server Error) if the update fails.</returns>
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var validation = new TokenRoleValidator(_context);

            var validationResult = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

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
            var createProcess = userProcess.UpdateUser(user);
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
