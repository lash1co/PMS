namespace Domain.Entities
{
    /// <summary>
    /// Prescription Entity definition
    /// </summary>
    public class Prescriptions
    {
        /// <summary>
        /// Id entifier value
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Medication for the patient
        /// </summary>
        public string Medication { get; set; } = string.Empty;

        /// <summary>
        /// DOsage for the medication
        /// </summary>
        public string Dosage { get; set; } = string.Empty;

        /// <summary>
        /// Medicatrion Refils if applies
        /// </summary>
        public int Refils { get; set; }

        /// <summary>
        /// Issue date
        /// </summary>
        public DateOnly IssueDate { get; set; }

        public Doctor Doctor { get; set; } = default!;

        public Patient Patient { get; set; } = default!;
    }
}