namespace Domain.Entities
{
    /// <summary>
    /// Represents the result of an upsert operation for a user, including the operation status and an optional message.
    /// </summary>
    public class UpsertRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether the upsert operation completed successfully.
        /// </summary>
        public bool UpsertSuccessfull { get; set; }

        /// <summary>
        /// Gets or sets the message content associated with this instance.
        /// </summary>
        public string? Message { get; set; }
    }
}
