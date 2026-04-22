using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers
{
    /// <summary>
    /// Provides functionality to validate JSON Web Tokens (JWT) from HTTP authorization headers and retrieve associated
    /// user roles.
    /// </summary>
    /// <remarks>Use this class to authenticate JWTs and extract user role information in scenarios where
    /// token-based authorization is required. The class does not throw exceptions for invalid tokens; instead, it
    /// returns validation results and messages to facilitate error handling and user feedback.</remarks>
    public class TokenRoleValidator
    {
        private readonly DatabaseContext _context;

        public TokenRoleValidator(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates a JSON Web Token (JWT) from the specified authorization header and retrieves the associated user
        /// role.
        /// </summary>
        /// <remarks>If the token is invalid or missing required claims, the returned role will be null or
        /// empty and the message will describe the validation failure. This method does not throw exceptions for
        /// invalid tokens; instead, it returns validation status and details in the result tuple.</remarks>
        /// <param name="authorizationHeader">The value of the HTTP Authorization header containing the JWT to validate. Must be in the format 'Bearer
        /// &lt;token&gt;'.</param>
        /// <param name="jwtKey">The secret key used to validate the JWT signature. Cannot be null or empty.</param>
        /// <param name="jwtIssuer">The expected issuer of the JWT. Used to verify the token's issuer claim.</param>
        /// <returns>A tuple containing a boolean indicating whether the token is valid, a string representing the user's role if
        /// validation succeeds, and a message describing the validation result.</returns>
        public async Task<(bool tokenIsValid, string role, string message)> ValidateTokenAndGetRole(string authorizationHeader, string jwtKey, string jwtIssuer)
        {
            var authenticationProcess = new AuthenticationProcess(_context);
            var tokenValidation = authenticationProcess.TokenValidation(authorizationHeader, jwtKey, jwtIssuer);
            var validationResult = await tokenValidation;
            return (validationResult.tokenIsValid, validationResult.role, validationResult.message);
        }
    }
}
