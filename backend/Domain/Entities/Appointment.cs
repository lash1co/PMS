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
        /// Current status of the appointment (e.g., Planned, Arrived, InProgress, Finished, Cancelled, EnteredInError).
        /// </summary>
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        /// <summary>
        /// The reason for the appointment or a short description of the visit.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// The doctor assigned to the appointment.
        /// </summary>
        public Doctor Doctor { get; set; } = default!;

        /// <summary>
        /// The foreign key for the doctor associated with the appointment.
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// The patient associated with the appointment.
        /// </summary>
        public Patient Patient { get; set; } = default!;

        /// <summary>
        /// Gets or sets the unique identifier for the patient.
        /// </summary>
        public int PatientId { get; set; }
    }
}