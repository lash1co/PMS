namespace Domain.Entities
{
    /// <summary>
    /// Represents a user account with identifying and contact information.
    /// </summary>
    /// <remarks>The User class encapsulates properties commonly associated with application users, including
    /// identifiers, credentials, and status. It can be used to store and transfer user-related data within
    /// authentication, authorization, or profile management scenarios.</remarks>
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address associated with the entity.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user name associated with the current instance.
        /// </summary>
        public string UserName {  get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password associated with the current user or entity.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the object is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the object was created.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets User's role definition for the application
        /// </summary>
        public string Role { get; set; }

        public Doctor? Doctor { get; set; }
    }
}
