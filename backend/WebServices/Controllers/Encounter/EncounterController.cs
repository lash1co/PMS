using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Encounter 
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EncounterController : ControllerBase
    {
        private readonly EncounterProcess _encounterProcess;
        private readonly IConfiguration _config;
        private readonly DatabaseContext _context;

        private readonly List<string> _authorizedRoles = new List<string>
        {
            UserConstants.RoleConstants.AdminRole,
            UserConstants.RoleConstants.DoctorRole
        };

        public EncounterController(EncounterProcess encounterProcess, IConfiguration config, DatabaseContext context)
        {
            _encounterProcess = encounterProcess;
            _config = config;
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of encounters for the authenticated doctor.
        /// </summary>
        /// <returns>A list of EncounterResponse objects.</returns>
        [HttpGet]
        public async Task<ActionResult<List<EncounterResponse>>> GetEncounters()
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var doctor = await _context.DBDoctors.FindAsync(authResult.Value.doctorId);
            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            var encounters = await _context.Encounters
                .Where(e => e.DoctorId == doctor.Id)
                .Select(e => new EncounterResponse
                {
                    EncounterId = e.Id,
                    PatientName = e.Patient.LastName + " " + e.Patient.FirstName,
                    StartTime = TimeOnly.FromDateTime(e.StartTime),
                    EndTime = e.EndTime.HasValue ? TimeOnly.FromDateTime(e.EndTime.Value) : TimeOnly.FromDateTime(e.StartTime),
                    EncounterReason = e.Appointment != null ? e.Appointment.Reason : string.Empty
                })
                .ToListAsync();

            return Ok(encounters);
        }

        /// <summary>
        /// Starts a new clinical encounter associated with an appointment.
        /// </summary>
        [HttpPost("start/{appointmentId}")]
        public async Task<IActionResult> StartEncounter(int appointmentId)
        {
            var result = await _encounterProcess.StartEncounterAsync(appointmentId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the complete summary of a specific encounter.
        /// </summary>
        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(int id)
        {
            var result = await _encounterProcess.GetEncounterSummaryAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Updates the clinical note (SOAP) for a specific encounter.
        /// </summary>
        [HttpPut("{id}/note")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateClinicalNoteRequest request)
        {
            var result = await _encounterProcess.UpdateClinicalNoteAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Completes the encounter and frees up the schedule.
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteEncounter(int id)
        {
            var result = await _encounterProcess.CompleteEncounterAsync(id);
            return Ok(result);
        }
    }
}