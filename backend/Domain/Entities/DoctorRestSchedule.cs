namespace Domain.Entities
{
    /// <summary>
    /// Represents a scheduled rest period for a doctor, including the time range and reason for the rest.
    /// </summary>
    public class DoctorRestSchedule
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the start time of the rest period. (TimeSpan for recurring daily times)
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the rest period. (TimeSpan for recurring daily times)
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Gets or sets the reason for the rest period.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Foreign Key for the Doctor
        /// </summary>
        public int DoctorId { get; set; }

        // Navigation property into Doctor's entity
        public Doctor Doctor { get; set; } = new Doctor();
    }
}