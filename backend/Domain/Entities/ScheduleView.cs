namespace Domain.Entities
{
    /// <summary>
    /// Represents a view model for a schedule event, including appointments and doctor rest periods.
    /// </summary>
    public  class ScheduleView
    {
        /// <summary>
        /// Gets or sets the type associated with the current instance.
        /// Valuyes must be one of the following: "Appointment", "Rest".
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// Values based on AppointmentId or DoctorRestScheduleId depending on the Type.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the doctor.
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// Gets or sets the name of the doctor associated with this instance.
        /// </summary>
        public string DoctorName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the patient associated with this record.
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the patient.
        /// </summary>
        public string? PatientName { get; set; }

        /// <summary>
        /// Gets or sets the date value associated with this instance.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the scheduled start time for the event.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for the event.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the current status of the schedule.
        /// </summary>
        public string ScheduleStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the schedule event.
        /// </summary>
        public string ScheduleDescription { get; set; } = string.Empty;
    }
}
