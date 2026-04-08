namespace Domain.Entities
{
    /// <summary>
    /// Represents a medical record for a patient containing visit information,
    /// clinical notes, diagnosis and treatment details.
    /// </summary>
    public class MedicalRecord
    {
        /// <summary>
        /// Unique identifier for the medical record.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Date of the patient visit.
        /// </summary>
        public DateOnly VisitDate { get; set; }

        /// <summary>
        /// Recorded vital signs or brief summary of vitals captured during the visit.
        /// Stored as a string to allow free-form entries (e.g. blood pressure, pulse, temperature).
        /// </summary>
        public string Vitals { get; set; } = string.Empty;

        /// <summary>
        /// Code identifying the diagnosis made during the visit (e.g. ICD code).
        /// </summary>
        public string DiagnosisCode { get; set; } = string.Empty;

        /// <summary>
        /// Proposed or prescribed treatment plan for the patient following the visit.
        /// </summary>
        public string TreatmentPlan { get; set; } = string.Empty;

        /// <summary>
        /// Detailed clinical notes recorded by the clinician about the visit.
        /// </summary>
        public string ClinicalNotes { get; set; } = string.Empty;

        /// <summary>
        /// The patient associated with this medical record.
        /// </summary>
        public Patient Patient { get; set; }
    }
}