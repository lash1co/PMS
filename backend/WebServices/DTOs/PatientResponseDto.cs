using System;

namespace WebServices.DTOs
{
    /// <summary>
    /// Data Transfer Object used to send patient data back to the client.
    /// Ensures that internal database entities and their circular references are not exposed.
    /// </summary>
    public class PatientResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}