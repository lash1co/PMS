namespace Domain.Entities
{
    /// <summary>
    /// Represents a scheduled rest period for a doctor, including the time range and reason for the rest.
    /// </summary>
    /// <remarks>Use this class to record and manage periods when a doctor is unavailable due to rest or other
    /// specified reasons. This type is typically used in scheduling or calendar systems to prevent appointments from
    /// being booked during the specified time range.</remarks>
    public class DoctorRestSchedule
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the start time of the rest period.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the rest period.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the reason for the rest period.
        /// </summary>
        public string Reason { get; set; }

        // Navigation property into Doctor's entity
        public Doctor Doctor { get; set; } = new Doctor();
    }
}
