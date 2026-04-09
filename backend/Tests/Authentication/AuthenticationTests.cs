using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
using WebServices.DataAccess;
using WebServices.SharedBusiness;
using Domain.Entities;

namespace Tests.Authentication
{
    public class AuthenticationTests
    {
        private DatabaseContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new DatabaseContext(options);

            dbContext.Users.Add(new User
            {
                Id = 1,
                UserName = "testuser",
                Password = "hashedpassword123",
                IsActive = true,
                Email = "test@pms.com",
                Name = "Test User",
                CreationDate = DateTime.UtcNow
            });

            dbContext.Users.Add(new User
            {
                Id = 2,
                UserName = "inactiveuser",
                Password = "hashedpassword123",
                IsActive = false,
                Email = "inactive@pms.com",
                Name = "Inactive User",
                CreationDate = DateTime.UtcNow
            });

            dbContext.SaveChanges();
            return dbContext;
        }

        [Fact]
        public async Task Authenticate_With_Valid_Credentials_Should_Return_Token()
        {
            // Arrange
            using var dbContext = GetInMemoryDbContext();
            var authProcess = new AuthenticationProcess(dbContext);
            var credentials = new LoginCreadentials { Username = "testuser", Password = "hashedpassword123" };
            string jwtKey = "esta_es_una_llave_super_secreta_para_pruebas_unitarias_12345!";
            string issuer = "PMS_Test_Issuer";

            // Act
            var token = await authProcess.Authenticate(credentials, jwtKey, issuer);

            // Assert
            Assert.False(string.IsNullOrEmpty(token)); // Verificamos que generó un string (el JWT)
        }

        [Fact]
        public async Task Authenticate_With_Invalid_Username_Should_Throw_Unauthorized()
        {
            // Arrange
            using var dbContext = GetInMemoryDbContext();
            var authProcess = new AuthenticationProcess(dbContext);
            var credentials = new LoginCreadentials { Username = "nobody", Password = "hashedpassword123" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authProcess.Authenticate(credentials, "key", "issuer")
            );
            Assert.Equal("Invalid username.", ex.Message);
        }

        [Fact]
        public async Task Authenticate_With_Inactive_User_Should_Throw_Unauthorized()
        {
            // Arrange
            using var dbContext = GetInMemoryDbContext();
            var authProcess = new AuthenticationProcess(dbContext);
            var credentials = new LoginCreadentials { Username = "inactiveuser", Password = "hashedpassword123" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authProcess.Authenticate(credentials, "key", "issuer")
            );
            Assert.Equal("User is not active, please contact support.", ex.Message);
        }

        [Fact]
        public async Task Authenticate_With_Wrong_Password_Should_Throw_Unauthorized()
        {
            // Arrange
            using var dbContext = GetInMemoryDbContext();
            var authProcess = new AuthenticationProcess(dbContext);
            var credentials = new LoginCreadentials { Username = "testuser", Password = "wrongpassword" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authProcess.Authenticate(credentials, "key", "issuer")
            );
            Assert.Equal("Password is incorrect.", ex.Message);
        }
    }
}