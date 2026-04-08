using Domain.Entities;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Authentication
{
    public class AuthenticationProcess
    {
        public static string Authenticate(LoginCreadentials credentials, string jwtKey, string issuer)
        {
            // This is a placeholder for the actual authentication logic.
            // In a real application, you would verify the credentials against a user store (e.g., database).
            if (credentials.Username == "admin" && credentials.Password == "password")
            {
                // Generate and return a JWT token or similar authentication token here.
                return GenerateToken(credentials.Username, jwtKey, issuer);
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }

        private static string GenerateToken(string username, string jwtKey, string issuer)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
             new Claim(JwtRegisteredClaimNames.Sub, username),
             new Claim(JwtRegisteredClaimNames.Email, "userInfo.EmailAddress"),
             new Claim("DateOfJoing", DateTime.Now.ToString("yyyy-MM-dd")),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
         };

            var token = new JwtSecurityToken(issuer,
                issuer,
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
