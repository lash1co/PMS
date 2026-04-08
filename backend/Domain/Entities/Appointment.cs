namespace Domain.Entities
{
    /// <summary>
    /// Represents a scheduled appointment between a patient and a doctor,
    /// including timing, status and reason for the visit.
    /// </summary>
    public class Appointment
    {
        /// <summary>
        /// Unique identifier for the appointment.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The scheduled start time of the appointment.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The scheduled end time of the appointment.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Current status of the appointment (for example "Scheduled", "Completed",
        /// "Cancelled" or "NoShow").
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// The reason for the appointment or a short description of the visit.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// The doctor assigned to the appointment.
        /// </summary>
        public Doctor Doctor { get; set; } = default!;

        /// <summary>
        /// The patient associated with the appointment.
        /// </summary>
        public Patient Patient { get; set; } = default!;
    }
}