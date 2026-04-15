namespace Domain.Entities
{
    /// <summary>
    /// Represents the structure for token validation result
    /// </summary>
    public class AuthorizationRequest
    {
        /// <summary>
        /// Gets or sets wether the incoming request token is valid
        /// </summary>
        public bool tokenIsValid { get; set; }

        /// <summary>
        /// Gets or sets user role after token is validated
        /// </summary>
        public string? role { get; set; }

        /// <summary>
        /// Gets or sets token validation message
        /// Null when token validation is correct
        /// </summary>
        public string? message { get; set; }
    }
}
