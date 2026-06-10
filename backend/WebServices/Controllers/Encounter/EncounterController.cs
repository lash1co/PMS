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
    //[Authorize]
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

        private readonly List<string> _doctorOnlyRoles = new List<string>
        {
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

        private async Task<(bool IsAuthorized, IActionResult? Error, int DoctorId, string UserName)> ValidateDoctorAuthorizationAsync()
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _doctorOnlyRoles);

            if (!authResult.Value.tokenIsValid)
            {
                return (false, StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage), 0, string.Empty);
            }

            if (!authResult.Value.doctorId.HasValue)
            {
                return (false, StatusCode(StatusCodes.Status403Forbidden, "Doctor profile not found for the authenticated user."), 0, authResult.Value.userName);
            }

            return (true, null, authResult.Value.doctorId.Value, authResult.Value.userName);
        }

        private IActionResult MutationError(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => StatusCode(StatusCodes.Status403Forbidden, ex.Message),
                InvalidOperationException => BadRequest(ex.Message),
                KeyNotFoundException => NotFound(ex.Message),
                _ => StatusCode(StatusCodes.Status500InternalServerError, ex.Message)
            };
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.StartEncounterAsync(appointmentId, auth.UserName, auth.DoctorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
        }
        /// <summary>
        /// Creates an encounter for a walk-in or emergency patient without a scheduled appointment.
        /// </summary>
        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkIn([FromBody] CreateWalkInRequest request)
        {
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.CreateWalkInEncounterAsync(request, auth.UserName, auth.DoctorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
        }

        /// <summary>
        /// Invalidates an encounter, marking it as "Entered in Error" with a justification.
        /// </summary>
        [HttpPost("{id}/invalidate")]
        public async Task<IActionResult> InvalidateEncounter(int id, [FromBody] InvalidateEncounterRequest request)
        {
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.InvalidateEncounterAsync(id, request, auth.UserName, auth.DoctorId);
                return result ? Ok(new { message = "Encounter invalidated." }) : NotFound();
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.UpdateClinicalNoteAsync(id, request, auth.DoctorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
        }

        /// <summary>
        /// Completes the encounter, marks the end time, and frees up the doctor's schedule.
        /// </summary>
        /// <param name="id">The unique identifier of the encounter to complete.</param>
        /// <returns>A boolean indicating the success of the completion operation.</returns>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteEncounter(int id)
        {
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.CompleteEncounterAsync(id, auth.UserName, auth.DoctorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.AddObservationAsync(id, request, auth.DoctorId);
                if (!result) return NotFound("Encounter not found.");
                return Ok(new { message = "Observation added successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.RemoveObservationAsync(id, observationId, auth.DoctorId);
                if (!result) return NotFound("Observation or Encounter not found.");
                return Ok(new { message = "Observation removed successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.AddAllergyAsync(id, request, auth.DoctorId);
                if (!result) return NotFound("Encounter not found.");
                return Ok(new { message = "Allergy added successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.RemoveAllergyAsync(id, allergyId, auth.DoctorId);
                if (!result) return NotFound("Allergy or Encounter not found.");
                return Ok(new { message = "Allergy removed successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.AddConditionAsync(id, request, auth.DoctorId);
                if (!result) return NotFound("Encounter not found.");
                return Ok(new { message = "Condition added successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.RemoveConditionAsync(id, conditionId, auth.DoctorId);
                if (!result) return NotFound("Condition or Encounter not found.");
                return Ok(new { message = "Condition removed successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.AddProcedureAsync(id, request, auth.DoctorId);
                if (!result) return NotFound("Encounter not found.");
                return Ok(new { message = "Procedure added successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.RemoveProcedureAsync(id, procedureId, auth.DoctorId);
                if (!result) return NotFound("Procedure or Encounter not found.");
                return Ok(new { message = "Procedure removed successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
        }

        [HttpPost("{id}/laboratories")]
        public async Task<IActionResult> AddLaboratoryRequest(int id, [FromBody] CreateLaboratoryRequestDto request)
        {
            var auth = await ValidateDoctorAuthorizationAsync();
            if (!auth.IsAuthorized) return auth.Error!;

            try
            {
                var result = await _encounterProcess.AddLaboratoryRequestAsync(id, request, auth.DoctorId);
                if (!result) return NotFound("Encounter not found.");
                return Ok(new { message = "Laboratory request created successfully." });
            }
            catch (Exception ex)
            {
                return MutationError(ex);
            }
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
