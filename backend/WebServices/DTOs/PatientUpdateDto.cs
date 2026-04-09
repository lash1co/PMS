namespace WebServices.DTOs
{
    /// <summary>
    /// DTO used to receive data from the client when updating a patient.
    /// </summary>
    public class PatientUpdateDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
    }
}