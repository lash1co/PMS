using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Prescription
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : Controller
    {
        private IConfiguration _config;
        private readonly DatabaseContext _context;

        public PrescriptionsController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        // ✅ GET ALL
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prescriptions>>> GetAll()
        {
            var authorizationHeader = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader))
                return Unauthorized("Invalid authorization token.");

            var validation = new TokenRoleValidator(_context);

            var result = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!result.tokenIsValid)
                return Unauthorized(new { Message = result.message });

            // 🔥 IMPORTANTE: Include
            var data = await _context.DBPrescriptions
                .Include(p => p.Medications)
                    .ThenInclude(pm => pm.Medication)
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .ToListAsync();

            if (data.Count == 0)
                return NoContent();

            return Ok(data);
        }

        // ✅ GET BY ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Prescriptions>> GetById(int id)
        {
            var prescription = await _context.DBPrescriptions
                .Include(p => p.Medications)
                    .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NoContent();

            return Ok(prescription);
        }

        // ✅ CREATE
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Prescriptions prescription)
        {
            var authorizationHeader = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader))
                return Unauthorized("Invalid authorization token.");

            var validation = new TokenRoleValidator(_context);

            var result = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!result.tokenIsValid)
                return Unauthorized(new { Message = result.message });

            if (result.role != UserConstants.RoleConstants.AdminRole)
                return StatusCode(StatusCodes.Status403Forbidden, "No permission.");

            _context.DBPrescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return Ok(prescription);
        }

        // ✅ UPDATE
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Prescriptions prescription)
        {
            var authorizationHeader = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader))
                return Unauthorized("Invalid authorization token.");

            var validation = new TokenRoleValidator(_context);

            var result = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!result.tokenIsValid)
                return Unauthorized(new { Message = result.message });

            if (result.role != UserConstants.RoleConstants.AdminRole)
                return StatusCode(StatusCodes.Status403Forbidden, "No permission.");

            var existing = await _context.DBPrescriptions
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.Id == prescription.Id);

            if (existing == null)
                return NotFound();

            // 🔥 actualizar campos básicos
            existing.IssueDate = prescription.IssueDate;
            existing.DoctorId = prescription.DoctorId;
            existing.PatientId = prescription.PatientId;
            existing.EncounterId = prescription.EncounterId;

            // 🔥 actualizar medications (simple approach)
            _context.PrescriptionMedications.RemoveRange(existing.Medications);

            existing.Medications = prescription.Medications;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // ✅ DELETE
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var authorizationHeader = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader))
                return Unauthorized("Invalid authorization token.");

            var validation = new TokenRoleValidator(_context);

            var result = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!result.tokenIsValid)
                return Unauthorized(new { Message = result.message });

            if (result.role != UserConstants.RoleConstants.AdminRole)
                return StatusCode(StatusCodes.Status403Forbidden, "No permission.");

            var prescription = await _context.DBPrescriptions
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound();

            _context.PrescriptionMedications.RemoveRange(prescription.Medications);
            _context.DBPrescriptions.Remove(prescription);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }
    }
}