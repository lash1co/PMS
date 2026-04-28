using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.Controllers;
using WebServices.DataAccess;

namespace WebServices.SharedBusiness
{
    public class TokenValidationProcess
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _dbContext;

        public TokenValidationProcess(IConfiguration config, DatabaseContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Helper method to centralize token and role validation logic.
        /// </summary>
        public async Task<(bool tokenIsValid, int errorStatus, string? errorMessage)?> ValidateAuthorizationAsync(string requestHeader, List<string> authorizedRoles)
        {
            var authorizationHeader = requestHeader;
            if (string.IsNullOrEmpty(authorizationHeader)) return (false, StatusCodes.Status401Unauthorized, "Invalid authorization token.");

            var validation = new TokenRoleValidator(_dbContext);
            var validationResult = await validation.ValidateTokenAndGetRole(authorizationHeader, _config["Jwt:Key"], _config["Jwt:Issuer"]);

            if (!validationResult.tokenIsValid)
            {
                return (false, StatusCodes.Status401Unauthorized, validationResult.message);
            }

            if (!authorizedRoles.Contains(validationResult.role))
            {
                return (false, StatusCodes.Status403Forbidden, validationResult.role + " does not have permission to execute this process.");
            }

            return (true, StatusCodes.Status200OK, null); // Authorization successful
        }
    }
}
