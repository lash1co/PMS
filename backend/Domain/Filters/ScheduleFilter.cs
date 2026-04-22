namespace Domain.Filters
{
    /// <summary>
    /// Represents a filter for querying schedules.
    /// </summary>
    public class ScheduleFilter
    {
        /// <summary>
        /// Gets or sets the identifiers of the doctors associated with this entity.
        /// </summary>
        public int[] DoctorsId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the date value associated with this instance.
        /// </summary>
        public DateOnly? Date { get; set; }

        /// <summary>
        /// Gets or sets the current status of the schedule.
        /// </summary>
        public string? ScheduleStatus { get; set; }

        /// <summary>
        /// Gets or sets the full name of the patient.
        /// </summary>
        public string? PatientName { get; set; }
    }
}
