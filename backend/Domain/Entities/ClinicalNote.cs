using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ClinicalNote
    {
        public int Id { get; set; }

        public int EncounterId { get; set; }
        public Encounter Encounter { get; set; } = null!;

        // SOAP format
        public string? Subjective { get; set; } // Lo que el paciente dice que siente
        public string? Objective { get; set; }   // Lo que el doctor observa físicamente
        public string? Assessment { get; set; }  // El análisis o diagnóstico descriptivo
        public string? Plan { get; set; }        // Plan de acción o tratamiento a seguir

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
