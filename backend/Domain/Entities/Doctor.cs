namespace Domain.Entities
{
    /// <summary>
    /// Represents a medical doctor or provider who can be assigned to appointments
    /// and can author prescriptions for patients.
    /// </summary>
    public class Doctor
    {
        /// <summary>
        /// Unique identifier for the doctor.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The full name of the doctor.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Collection of appointments associated with the doctor. May be null if
        /// appointments have not been loaded or the doctor has none.
        /// </summary>
        public ICollection<Appointment>? Appointments { get; set; }

        /// <summary>
        /// Collection of prescriptions authored by the doctor. May be null if
        /// prescriptions have not been loaded or the doctor has none.
        /// </summary>
        public ICollection<Prescriptions>? Prescriptions { get; set; }
    }
}