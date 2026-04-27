using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ClinicalObservation
    {
        public int Id { get; set; }

        public int EncounterId { get; set; }
        public Encounter Encounter { get; set; } = null!;

        public int PatientId { get; set; }

        // Ej: "vital-signs"
        public string Category { get; set; } = string.Empty;

        // Ej: "8310-5" (Código LOINC para temperatura)
        public string Code { get; set; } = string.Empty;

        // Ej: "Temperatura Corporal"
        public string DisplayName { get; set; } = string.Empty;

        // El valor numérico, ej: 37.5
        public decimal? ValueQuantity { get; set; }

        // Para valores como "120/80" (Presión arterial)
        public string? ValueString { get; set; }

        // Ej: "Celsius", "mmHg", "kg"
        public string? Unit { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    }
}
