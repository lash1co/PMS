namespace Domain.Entities
{
    /// <summary>
    /// Prescription Entity definition
    /// </summary>
    public class Prescriptions
    {
        /// <summary>
        /// <summary>
        /// Unique identifier for the prescription.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The medication prescribed to the patient (for example, the drug name).
        /// </summary>
        public string Medication { get; set; } = string.Empty;

        /// <summary>
        /// Dosage instructions for the medication (for example "500 mg, twice daily").
        /// </summary>
        public string Dosage { get; set; } = string.Empty;

        /// <summary>
        /// Number of refills authorized for this prescription, if any.
        /// </summary>
        public int Refils { get; set; }

        /// <summary>
        /// The date the prescription was issued.
        /// </summary>
        public DateOnly IssueDate { get; set; }

        /// <summary>
        /// The doctor who created or authorized this prescription.
        /// </summary>
        public Doctor Doctor { get; set; } = default!;

        /// <summary>
        /// The patient for whom this prescription has been written.
        /// </summary>
        public Patient Patient { get; set; } = default!;

        // Encounter FK
        public int? EncounterId { get; set; }
        public Encounter? Encounter { get; set; }
    }
}