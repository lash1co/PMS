using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a patient entity within the domain model.
    /// Acts as a data container (Anemic Domain Model) mapped to the database.
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// The unique identifier for the patient.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets User Fisrtname
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Gets or sets User Lastname
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// The patient's date of birth. Used to calculate age and validate minor status.
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets User's phone number
        /// </summary>
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Gets or sets User's email
        /// </summary>
        public string? Email { get; set; }

        // public string MedicalRecordNumber { get; private set; } = null!;

        /// <summary>
        /// The timestamp indicating when the patient record was created. Defaults to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        /// <summary>
        /// Collection of medical records associated with the patient.
        /// </summary>
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }

        /// <summary>
        /// Collection of prescriptions issued to the patient.
        /// </summary>
        public ICollection<Prescriptions>? Prescriptions { get; set; }

        /// <summary>
        /// Collection of appointments scheduled for the patient.
        /// </summary>
        public ICollection<Appointment>? Appointments { get; set; }

        /// <summary>
        /// Collection of financial invoices billed to the patient.
        /// </summary>
        public ICollection<Invoice>? Invoices { get; set; }

        /// <summary>
        /// Parameterless constructor required by Entity Framework Core.
        /// </summary>
        public Patient() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Patient"/> class with essential details.
        /// </summary>
        /// <param name="firstName">The patient's first name.</param>
        /// <param name="lastName">The patient's last name.</param>
        /// <param name="dateOfBirth">The patient's date of birth.</param>
        /// <param name="phone">The primary contact phone number.</param>
        /// <param name="email">The optional email address.</param>
        public Patient(
            string firstName,
            string lastName,
            DateTime dateOfBirth,
            string phone,
            string? email = null)
        {
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Phone = phone;
            Email = email;
        }
    }
}