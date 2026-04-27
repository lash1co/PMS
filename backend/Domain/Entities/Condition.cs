using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a clinical condition, problem, diagnosis, or other event/situation that is an issue for the patient.
    /// </summary>
    public class Condition
    {
        /// <summary>Unique identifier for the condition record.</summary>
        public int Id { get; set; }

        /// <summary>The encounter during which this condition was diagnosed or evaluated.</summary>
        public int EncounterId { get; set; }
        public Encounter Encounter { get; set; } = null!;

        /// <summary>The identifier of the patient associated with this condition.</summary>
        public int PatientId { get; set; }

        /// <summary>A standardized terminology code representing the condition (e.g., ICD-10 code "J01.90").</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>A human-readable name for the condition (e.g., "Acute Sinusitis").</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>The clinical tracking status of the condition.</summary>
        public ConditionClinicalStatus ClinicalStatus { get; set; } = ConditionClinicalStatus.Active;

        /// <summary>The date and time when this condition was officially recorded.</summary>
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
    }
}