namespace Domain.Entities
{
    /// <summary>
    /// Represents a set of user credentials used for authentication during login operations.
    /// </summary>
    public class LoginCreadentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
