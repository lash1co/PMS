using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.SharedBusiness;
using Domain.Entities;
using Domain.Exceptions;

namespace WebServices.Controllers.Patients
{

    public record PatientRequestRecord(string FirstName, string LastName, DateTime DateOfBirth, string Phone, string? Email);
    public record InvoiceRequestRecord(decimal Amount, DateTime DueDate);
    public record PatientResponseRecord(int Id, string FirstName, string LastName, DateTime DateOfBirth, string Phone, string? Email, DateTime CreatedAt);
    /// <summary>
    /// API Controller responsible for managing patient data and their related entities, such as invoices.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly PatientProcess _patientProcess;

        /// <summary>
        /// Initializes a new instance of the PatientsController.
        /// </summary>
        public PatientsController(DatabaseContext dbContext, PatientProcess patientProcess)
        {
            _dbContext = dbContext;
            _patientProcess = patientProcess;
        }

        /// <summary>
        /// Retrieves a list of all registered patients.
        /// </summary>
        /// <returns>A collection of patients.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            return await _dbContext.DBPatients.ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific patient by their unique identifier, including their invoices.
        /// </summary>
        /// <param name="id">The unique identifier of the patient.</param>
        /// <returns>The patient details if found; otherwise, a 404 Not Found response.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(int id)
        {
            var patient = await _dbContext.DBPatients
                .Include(p => p.Invoices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound(new { message = "Patient not found." });
            }

            return patient;
        }

        /// <summary>
        /// Searches for patients by matching multiple words in their first or last name, 
        /// ignoring accents and case. Allows searching "FirstName LastName" or viceversa.
        /// </summary>
        /// <param name="searchTerm">The text to search for.</param>
        /// <returns>A list of patients matching all search terms.</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Patient>>> SearchPatients([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _dbContext.DBPatients.ToListAsync();
            }

            string collation = "SQL_Latin1_General_CP1_CI_AI";

            var query = _dbContext.DBPatients.AsQueryable();

            var terms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var term in terms)
            {
                query = query.Where(p =>
                    EF.Functions.Collate(p.FirstName, collation).Contains(term) ||
                    EF.Functions.Collate(p.LastName, collation).Contains(term));
            }

            var patients = await query.ToListAsync();

            return Ok(patients);
        }

        /// <summary>
        /// Creates a new patient record in the system.
        /// </summary>
        /// <param name="patientDto">The data required to create a new patient.</param>
        /// <returns>The newly created patient data.</returns>
        [HttpPost]
        public async Task<ActionResult<PatientResponseRecord>> CreatePatient([FromBody] PatientRequestRecord request)
        {
            try
            {
                var newPatient = _patientProcess.CreatePatient(
                    request.FirstName,
                    request.LastName,
                    request.DateOfBirth,
                    request.Phone,
                    request.Email
                );

                _dbContext.DBPatients.Add(newPatient);
                await _dbContext.SaveChangesAsync();

                var response = new PatientResponseRecord(
                    newPatient.Id, newPatient.FirstName, newPatient.LastName,
                    newPatient.DateOfBirth, newPatient.Phone, newPatient.Email, newPatient.CreatedAt
                );

                return Ok(response);
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates the details of an existing patient using business rules.
        /// </summary>
        /// <param name="id">The unique identifier of the patient to update.</param>
        /// <param name="patientDto">The updated data for the patient.</param>
        /// <returns>A success message along with the updated patient data (DTO).</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientRequestRecord request)
        {
            var patientEntity = await _dbContext.DBPatients.FindAsync(id);
            if (patientEntity == null) return NotFound(new { message = "Patient not found." });

            try
            {
                _patientProcess.UpdateDetails(patientEntity, request.FirstName, request.LastName, request.DateOfBirth, request.Phone, request.Email);
                await _dbContext.SaveChangesAsync();

                var response = new PatientResponseRecord(
                    patientEntity.Id, patientEntity.FirstName, patientEntity.LastName,
                    patientEntity.DateOfBirth, patientEntity.Phone, patientEntity.Email, patientEntity.CreatedAt
                );

                return Ok(response);
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Creates and associates a new invoice for a specific patient.
        /// </summary>
        /// <param name="id">The unique identifier of the patient.</param>
        /// <param name="invoiceDto">The data required to create the invoice.</param>
        /// <returns>A success message and the new invoice ID.</returns>
        [HttpPost("{id}/invoices")]
        public async Task<IActionResult> CreateInvoiceForPatient(int id, [FromBody] InvoiceRequestRecord request)
        {
            var patientEntity = await _dbContext.DBPatients
                .Include(p => p.Invoices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patientEntity == null)
            {
                return NotFound(new { message = "Patient not found." });
            }

            try
            {
                var newInvoice = _patientProcess.CreateInvoice(
                    patientEntity,
                    request.Amount,
                    request.DueDate
                );

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Invoice created successfully.", invoiceId = newInvoice.Id });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific patient from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the patient to delete.</param>
        /// <returns>A 204 No Content response if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _dbContext.DBPatients.FindAsync(id);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found." });
            }

            _dbContext.DBPatients.Remove(patient);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}