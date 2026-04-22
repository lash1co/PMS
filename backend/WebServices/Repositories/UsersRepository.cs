using Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using WebServices.DataAccess;

namespace WebServices.Repositories
{
    /// <summary>
    /// Provides methods for managing user data in the database.
    /// </summary>
    /// <remarks>The UsersRepository class encapsulates data access operations related to users, enabling the
    /// creation and management of user records. It is intended to be used as a data access layer component within
    /// applications that interact with a database context.</remarks>
    public class UsersRepository
    {
        private readonly DatabaseContext _dbContext;

        public UsersRepository(DataAccess.DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new user in the database with a securely hashed password.
        /// </summary>
        /// <remarks>The method hashes the user's password using SHA-256 before storing it. The
        /// CreationDate property is set to the current UTC time. If an error occurs during creation, the returned
        /// UserUpsertRequest will indicate failure and include the error message.</remarks>
        /// <param name="user">The user entity to create. The Password property must contain the plain text password to be hashed. Cannot
        /// be null.</param>
        /// <returns>A UserUpsertRequest indicating whether the user was created successfully and containing a status message.</returns>
        public async Task<UserUpsertRequest> CreateUser(User user)
        {
            try
            {
                var inputBytes = Encoding.UTF8.GetBytes(user.Password);
                var hashBytes = SHA256.HashData(inputBytes);
                user.Password = Convert.ToHexString(hashBytes);
                user.CreationDate = DateTime.UtcNow;

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                return new UserUpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "User created successfully."
                };
            }
            catch (Exception ex)
            {
                return new UserUpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error creating user: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Updates the details of an existing user in the data store.
        /// </summary>
        /// <remarks>If the specified user does not exist, the operation fails and an appropriate message
        /// is returned. The method does not throw exceptions; errors are reported in the returned
        /// UserUpsertRequest.</remarks>
        /// <param name="user">The user entity containing updated information. The user's Id must correspond to an existing user. Cannot be
        /// null.</param>
        /// <returns>A UserUpsertRequest indicating whether the update was successful and providing a descriptive message.</returns>
        public async Task<UserUpsertRequest> UpdateUser(User user)
        {
            try
            {
                var existingUser = await _dbContext.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    return new UserUpsertRequest
                    {
                        UpsertSuccessfull = false,
                        Message = "User not found."
                    };
                }
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.IsActive = user.IsActive;
                existingUser.Role = user.Role;

                await _dbContext.SaveChangesAsync();
                return new UserUpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "User updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new UserUpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error updating user: {ex.Message}"
                };
            }
        }
    }
}
