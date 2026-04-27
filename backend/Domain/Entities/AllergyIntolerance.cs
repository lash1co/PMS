using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    /// <summary>
    /// Represents an adverse reaction, allergy, or intolerance identified for a patient during a clinical encounter.
    /// </summary>
    public class AllergyIntolerance
    {
        /// <summary>Unique identifier for the allergy record.</summary>
        public int Id { get; set; }

        /// <summary>The identifier of the patient experiencing the allergy.</summary>
        public int PatientId { get; set; }

        /// <summary>The identifier of the encounter where this allergy was discovered or recorded.</summary>
        public int EncounterId { get; set; }
        public Encounter Encounter { get; set; } = null!;

        /// <summary>The specific substance causing the reaction (e.g., "Penicillin", "Peanuts", "Dust").</summary>
        public string Substance { get; set; } = string.Empty;

        /// <summary>An estimate of the potential clinical harm, or seriousness, of the reaction.</summary>
        public AllergyCriticality Criticality { get; set; } = AllergyCriticality.Low;

        /// <summary>A clinical description of the adverse reaction (e.g., "Skin rash", "Anaphylaxis").</summary>
        public string Reaction { get; set; } = string.Empty;

        /// <summary>The date and time when the allergy was recorded in the system.</summary>
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
    }
}