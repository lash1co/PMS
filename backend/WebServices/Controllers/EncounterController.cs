using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncounterController : Controller
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _context;
        private readonly List<string> _authorizedRoles = new List<string>
        {
            UserConstants.RoleConstants.AdminRole,
            UserConstants.RoleConstants.DoctorRole
        };

        public EncounterController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of encounters for the authenticated doctor.
        /// </summary>
        /// <returns></returns>
        [Authorize]
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
    }
}
