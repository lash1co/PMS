namespace Domain.Entities
{
    /// <summary>
    /// Encapsulates the response details for a clinical encounter, including identifiers, patient information, timing, and reason for the encounter.
    /// </summary>
    public class EncounterResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the encounter.
        /// </summary>
        public int EncounterId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the patient.
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start time for the associated event or operation.
        /// </summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for the associated event or operation.
        /// </summary>
        public TimeOnly EndTime { get; set; }

        /// <summary>
        /// Gets or sets the reason for the encounter, providing context for the event or operation.
        /// </summary>
        public string EncounterReason { get; set; } = string.Empty;
    }
}
