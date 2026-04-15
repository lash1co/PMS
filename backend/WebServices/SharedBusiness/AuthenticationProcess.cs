using Domain.Entities;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;


namespace WebServices.SharedBusiness
{
    public class AuthenticationProcess
    {
        private readonly DataAccess.DatabaseContext _dbContext;

        public AuthenticationProcess(DataAccess.DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> Authenticate(LoginCreadentials credentials, string jwtKey, string issuer)
        {
            var user = await _dbContext.Users
                .Where(u => u.UserName == credentials.Username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User is not active, please contact support.");
            }

            if (user.Password != credentials.Password)
            {
                throw new UnauthorizedAccessException("Password is incorrect.");
            }

            // Once he user is validated, we generate and return the authorization TOKEN.
            return GenerateToken(user, jwtKey, issuer);
        }

        /// <summary>
        /// Validates the token inside Http Request
        /// </summary>
        /// <param name="requestHeader"></param>
        /// <param name="secretKey"></param>
        /// <param name="issuer"></param>
        /// <returns>Authorization Request with validation result</returns>
        public async Task<AuthorizationRequest> TokenValidation(string requestHeader, string secretKey, string issuer)
        {
            var token = GetIncomingToken(requestHeader);
            if (token == null)
            {
                return new AuthorizationRequest
                {
                    tokenIsValid = false,
                    message = "Invalid authorization token"
                };
            }

            var validationParametters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var audiience = jwtToken.Audiences.FirstOrDefault();
            var expiration = jwtToken.ValidTo;
            var user = jwtToken.Claims.FirstOrDefault(t => t.Type == "sub")?.Value;

            try
            {
                var validation = tokenHandler.ValidateToken(token, validationParametters, out SecurityToken validatedToken);

                //Token is valid, we must get and return user's profile
                if (expiration < DateTime.UtcNow)
                {
                    return new AuthorizationRequest
                    {
                        tokenIsValid = false,
                        message = "Token expired."
                    };
                }

                var dbUser = await _dbContext.Users
                    .Where(u => u.UserName == user)
                    .FirstOrDefaultAsync();
                if (dbUser == null)
                {
                    return new AuthorizationRequest
                    {
                        tokenIsValid = false,
                        message = "Invalid user"
                    };
                }

                if (!dbUser.IsActive)
                {
                    return new AuthorizationRequest
                    {
                        tokenIsValid = false,
                        message = "User is disabled, pleae contact an Admin user to enable him."
                    };
                }

                return new AuthorizationRequest
                {
                    tokenIsValid = true,
                    role = dbUser.Role,
                };
            }
            catch (Exception ex)
            {
                return new AuthorizationRequest
                {
                    tokenIsValid = false,
                    message = ex.Message,
                };
            }
        }

        private static string GenerateToken(User user, string jwtKey, string issuer)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
             new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
             new Claim(JwtRegisteredClaimNames.Email, user.Email),
             new Claim("Name", user.Name),
             new Claim("DateOfJoing", user.CreationDate.ToString("yyyy-MM-dd")),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
         };

            var token = new JwtSecurityToken(issuer,
                issuer,
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string? GetIncomingToken(string requestHeader)
        {
            if (requestHeader.StartsWith("Bearer "))
            {
                return requestHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}
