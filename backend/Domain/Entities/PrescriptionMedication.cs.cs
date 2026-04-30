using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class PrescriptionMedication
    {
        public int Id { get; set; }

        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public int Refills { get; set; }

        // FK
        public int PrescriptionId { get; set; }
        public Prescriptions Prescription { get; set; } = default!;
    }
}
