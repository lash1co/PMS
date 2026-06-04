using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Encounter
{
    /// <summary>
    /// Handles HTTP requests related to clinical encounters.
    /// Manages the lifecycle of an encounter, clinical notes, and the association of medical records 
    /// (observations, allergies, conditions, and procedures).
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterController"/> class.
        /// </summary>
        /// <param name="encounterProcess">The business logic process for handling encounters.</param>
        /// <param name="config">The application configuration properties.</param>
        /// <param name="context">The database context for data access.</param>
        public EncounterController(EncounterProcess encounterProcess, IConfiguration config, DatabaseContext context)
        {
            _encounterProcess = encounterProcess;
            _config = config;
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of encounters for the authenticated doctor.
        /// </summary>
        /// <returns>A list of <see cref="EncounterResponse"/> objects containing basic encounter details.</returns>
        /// <response code="200">Returns the list of encounters successfully.</response>
        /// <response code="401">If the authorization token is invalid or missing.</response>
        /// <response code="404">If the authenticated doctor is not found in the database.</response>
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
                .OrderByDescending(e => e.StartTime)
                .Select(e => new EncounterResponse
                {
                    EncounterId = e.Id,
                    PatientName = e.Patient.LastName + " " + e.Patient.FirstName,
                    DoctorName = doctor.Name,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    EncounterReason = e.Appointment != null ? e.Appointment.Reason : (e.StatusReason ?? "Walk-In / Emergency"),
                    Status = e.Status.ToString()
                })
                .ToListAsync();

            return Ok(encounters);
        }

        /// <summary>
        /// Starts a new clinical encounter associated with a scheduled appointment.
        /// </summary>
        /// <param name="appointmentId">The unique identifier of the appointment.</param>
        /// <returns>A summary of the newly started encounter.</returns>
        [HttpPost("start/{appointmentId}")]
        public async Task<IActionResult> StartEncounter(int appointmentId)
        {
            
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid) return Unauthorized();

            var result = await _encounterProcess.StartEncounterAsync(appointmentId, authResult.Value.userName);
            return Ok(result);
        }
        /// <summary>
        /// Creates an encounter for a walk-in or emergency patient without a scheduled appointment.
        /// </summary>
        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkIn([FromBody] CreateWalkInRequest request)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid) return Unauthorized();

            var result = await _encounterProcess.CreateWalkInEncounterAsync(request, authResult.Value.userName);
            return Ok(result);
        }

        /// <summary>
        /// Invalidates an encounter, marking it as "Entered in Error" with a justification.
        /// </summary>
        [HttpPost("{id}/invalidate")]
        public async Task<IActionResult> InvalidateEncounter(int id, [FromBody] InvalidateEncounterRequest request)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid) return Unauthorized();

            var result = await _encounterProcess.InvalidateEncounterAsync(id, request, authResult.Value.userName);
            return result ? Ok(new { message = "Encounter invalidated." }) : NotFound();
        }

        /// <summary>
        /// Retrieves the complete summary of a specific encounter, including all attached clinical records.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <returns>An <see cref="EncounterSummaryDto"/> containing the full details of the encounter.</returns>
        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(int id)
        {
            var result = await _encounterProcess.GetEncounterSummaryAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Updates the clinical narrative note (SOAP note) for a specific encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="request">The updated clinical note sections (Subjective, Objective, Assessment, Plan).</param>
        /// <returns>A boolean indicating the success of the operation.</returns>
        [HttpPut("{id}/note")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateClinicalNoteRequest request)
        {
            var result = await _encounterProcess.UpdateClinicalNoteAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Completes the encounter, marks the end time, and frees up the doctor's schedule.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter to complete.</param>
        /// <returns>A boolean indicating the success of the completion operation.</returns>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteEncounter(int id)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid) return Unauthorized();

            var result = await _encounterProcess.CompleteEncounterAsync(id, authResult.Value.userName);
            return Ok(result);
        }

        // ==========================================
        // CLINICAL OBSERVATIONS
        // ==========================================

        /// <summary>
        /// Adds a new clinical observation (e.g., vital signs) to the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="request">The data for the new observation.</param>
        /// <returns>A success message if added correctly, or NotFound if the encounter doesn't exist.</returns>
        [HttpPost("{id}/observations")]
        public async Task<IActionResult> AddObservation(int id, [FromBody] CreateObservationDto request)
        {
            var result = await _encounterProcess.AddObservationAsync(id, request);
            if (!result) return NotFound("Encounter not found.");
            return Ok(new { message = "Observation added successfully." });
        }

        /// <summary>
        /// Removes a specific clinical observation from the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="observationId">The unique identifier of the observation to remove.</param>
        /// <returns>A success message if removed correctly, or NotFound if not found.</returns>
        [HttpDelete("{id}/observations/{observationId}")]
        public async Task<IActionResult> RemoveObservation(int id, int observationId)
        {
            var result = await _encounterProcess.RemoveObservationAsync(id, observationId);
            if (!result) return NotFound("Observation or Encounter not found.");
            return Ok(new { message = "Observation removed successfully." });
        }

        // ==========================================
        // ALLERGIES
        // ==========================================

        /// <summary>
        /// Adds a new allergy or intolerance record to the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="request">The data for the new allergy.</param>
        /// <returns>A success message if added correctly, or NotFound if the encounter doesn't exist.</returns>
        [HttpPost("{id}/allergies")]
        public async Task<IActionResult> AddAllergy(int id, [FromBody] CreateAllergyDto request)
        {
            var result = await _encounterProcess.AddAllergyAsync(id, request);
            if (!result) return NotFound("Encounter not found.");
            return Ok(new { message = "Allergy added successfully." });
        }

        /// <summary>
        /// Removes a specific allergy or intolerance record from the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="allergyId">The unique identifier of the allergy to remove.</param>
        /// <returns>A success message if removed correctly, or NotFound if not found.</returns>
        [HttpDelete("{id}/allergies/{allergyId}")]
        public async Task<IActionResult> RemoveAllergy(int id, int allergyId)
        {
            var result = await _encounterProcess.RemoveAllergyAsync(id, allergyId);
            if (!result) return NotFound("Allergy or Encounter not found.");
            return Ok(new { message = "Allergy removed successfully." });
        }

        // ==========================================
        // CONDITIONS
        // ==========================================

        /// <summary>
        /// Adds a new medical condition (diagnosis) to the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="request">The data for the new condition.</param>
        /// <returns>A success message if added correctly, or NotFound if the encounter doesn't exist.</returns>
        [HttpPost("{id}/conditions")]
        public async Task<IActionResult> AddCondition(int id, [FromBody] CreateConditionDto request)
        {
            var result = await _encounterProcess.AddConditionAsync(id, request);
            if (!result) return NotFound("Encounter not found.");
            return Ok(new { message = "Condition added successfully." });
        }

        /// <summary>
        /// Removes a specific medical condition from the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="conditionId">The unique identifier of the condition to remove.</param>
        /// <returns>A success message if removed correctly, or NotFound if not found.</returns>
        [HttpDelete("{id}/conditions/{conditionId}")]
        public async Task<IActionResult> RemoveCondition(int id, int conditionId)
        {
            var result = await _encounterProcess.RemoveConditionAsync(id, conditionId);
            if (!result) return NotFound("Condition or Encounter not found.");
            return Ok(new { message = "Condition removed successfully." });
        }

        // ==========================================
        // PROCEDURES
        // ==========================================

        /// <summary>
        /// Adds a new medical procedure record to the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="request">The data for the new procedure.</param>
        /// <returns>A success message if added correctly, or NotFound if the encounter doesn't exist.</returns>
        [HttpPost("{id}/procedures")]
        public async Task<IActionResult> AddProcedure(int id, [FromBody] CreateProcedureDto request)
        {
            var result = await _encounterProcess.AddProcedureAsync(id, request);
            if (!result) return NotFound("Encounter not found.");
            return Ok(new { message = "Procedure added successfully." });
        }

        /// <summary>
        /// Removes a specific medical procedure record from the encounter.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter.</param>
        /// <param name="procedureId">The unique identifier of the procedure to remove.</param>
        /// <returns>A success message if removed correctly, or NotFound if not found.</returns>
        [HttpDelete("{id}/procedures/{procedureId}")]
        public async Task<IActionResult> RemoveProcedure(int id, int procedureId)
        {
            var result = await _encounterProcess.RemoveProcedureAsync(id, procedureId);
            if (!result) return NotFound("Procedure or Encounter not found.");
            return Ok(new { message = "Procedure removed successfully." });
        }

        /// ==========================================
        /// HISTORY
        /// ==========================================

        /// <summary>
        /// Retrieves the historical list of completed encounters based on specific filters.
        /// </summary>
        /// <param name="filter">The filtering parameters from the query string.</param>
        /// <returns>A prioritized list of encounter history records.</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] EncounterHistoryFilterDto filter)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid) return Unauthorized();

            var result = await _encounterProcess.GetEncounterHistoryAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the comprehensive details of a specific historical encounter.
        /// </summary>
        /// <param name="id">The encounter identifier.</param>
        /// <returns>Detailed encounter and appointment data.</returns>
        [HttpGet("{id}/history-detail")]
        public async Task<IActionResult> GetHistoryDetail(int id)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid) return Unauthorized();

            var result = await _encounterProcess.GetEncounterHistoryDetailAsync(id);
            return Ok(result);
        }
    }
}