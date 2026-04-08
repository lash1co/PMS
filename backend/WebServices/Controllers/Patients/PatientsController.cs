using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.DTOs;

namespace WebServices.Controllers.Patients
{
    /// <summary>
    /// Controller responsible for managing patient-related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] //
    public class PatientsController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public PatientsController(DatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all registered patients.
        /// </summary>
        /// <returns>A status 200 OK containing the list of patients.</returns>
        // GET: api/patients
        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _context.DBPatients.ToListAsync();
            return Ok(patients);
        }

        /// <summary>
        /// Retrieves a specific patient by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the patient.</param>
        /// <returns>The patient details if found; otherwise, a 404 Not Found response.</returns>
        // GET: api/patients/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _context.DBPatients.FindAsync(id);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            return Ok(patient);
        }

        /// <summary>
        /// Searches for patients whose first name or last name contains the specified search term,
        /// ignoring case and accents.
        /// </summary>
        /// <param name="searchTerm">The term to search for within the patients' names.</param>
        /// <returns>A list of matching patients or a 404 Not Found response if no matches exist.</returns>
        // GET: api/patients/search?searchTerm=Name
        [HttpGet("search")]
        public async Task<IActionResult> SearchPatients([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("The search term cannot be empty.");
            }

            var term = searchTerm.Trim();

            var patients = await _context.DBPatients
                .Where(p =>
                    EF.Functions.Collate(p.FirstName, "SQL_Latin1_General_CP1_CI_AI").Contains(term) ||
                    EF.Functions.Collate(p.LastName, "SQL_Latin1_General_CP1_CI_AI").Contains(term))
                .ToListAsync();

            if (!patients.Any())
            {
                return NotFound($"No patients found matching the term '{searchTerm}'.");
            }

            return Ok(patients);
        }

        /// <summary>
        /// Registers a new patient in the system.
        /// </summary>
        /// <param name="patient">The patient object containing the details to be registered.</param>
        /// <returns>A 201 Created response with the newly created patient data.</returns>
        // POST: api/patients
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            try
            {
                _context.DBPatients.Add(patient);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, patient);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing patient's information.
        /// </summary>
        // PUT: api/patients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpdateDto patientDto)
        {
            // 1. Validamos que el ID de la ruta coincida con el DTO
            if (id != patientDto.Id)
            {
                return BadRequest("The ID in the URL does not match the ID of the patient to update.");
            }

            // 2. Buscamos el paciente existente en la base de datos
            var existingPatient = await _context.DBPatients.FindAsync(id);
            if (existingPatient == null)
            {
                return NotFound("The patient you are trying to update no longer exists.");
            }

            try
            {
                // 3. Actualizamos las propiedades usando el método controlado del Dominio (Encapsulamiento)
                existingPatient.UpdateDetails(
                    patientDto.FirstName,
                    patientDto.LastName,
                    patientDto.DateOfBirth,
                    patientDto.Phone,
                    patientDto.Email
                );

                // 4. Guardamos los cambios. Entity Framework ya está rastreando a 'existingPatient'
                await _context.SaveChangesAsync();
            }
            catch (Domain.Exceptions.DomainException ex)
            {
                // Si falla alguna validación de negocio (ej. fecha futura), regresamos el error
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a patient from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the patient to delete.</param>
        /// <returns>A 204 No Content response if successful, or a 404 Not Found response if the patient does not exist.</returns>
        // DELETE: api/patients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.DBPatients.FindAsync(id);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            _context.DBPatients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if a patient exists in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the patient.</param>
        /// <returns>True if the patient exists; otherwise, false.</returns>
        private bool PatientExists(int id)
        {
            return _context.DBPatients.Any(e => e.Id == id);
        }
    }
}