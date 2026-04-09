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
    }
}
