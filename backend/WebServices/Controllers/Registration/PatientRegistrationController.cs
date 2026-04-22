using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Registration
{
    public record InsuranceRequestRecord(string PayerName, string MemberId, string? PlanType, string RelationshipToSubscriber);
    public record CompleteRegistrationRequestRecord(Patients.PatientRequestRecord PersonalInfo, InsuranceRequestRecord InsuranceInfo);

    /// <summary>
    /// Use-Case Controller: Management of patient registration, including generation of invitation links (Magic Links) and completion of patient profile.
    /// Patient Profile Completion includes saving personal details and insurance information. This controller orchestrates the necessary processes to ensure data integrity and proper flow of the registration process.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PatientRegistrationController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly PatientProcess _patientProcess;
        private readonly InsuranceProcess _insuranceProcess;
        private readonly AuthenticationProcess _authProcess;
        private readonly IConfiguration _configuration;

        private readonly IMemoryCache _cache;

        public PatientRegistrationController(
            DatabaseContext dbContext,
            PatientProcess patientProcess,
            InsuranceProcess insuranceProcess,
            AuthenticationProcess authProcess,
            IConfiguration configuration, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _patientProcess = patientProcess;
            _insuranceProcess = insuranceProcess;
            _authProcess = authProcess;
            _configuration = configuration;
            _cache = cache;
        }

        /// <summary>
        /// Generate a unique invitation link (Magic Link) for a patient, which can be used to complete their registration. The link contains a JWT token that encodes the patient's ID and has an expiration time for security purposes.
        /// </summary>
        [HttpPost("{id}/generate-invite")]
        public async Task<IActionResult> GenerateInviteLink(int id)
        {
            var patient = await _dbContext.DBPatients.FindAsync(id);
            if (patient == null) return NotFound(new { message = "Patient not found." });

            string jwtKey = _configuration["Jwt:Key"];
            string issuer = _configuration["Jwt:Issuer"];
            
            // 1. Obtener Token y JTI
            var (token, jti) = _authProcess.GeneratePatientInvitationToken(id, jwtKey, issuer);

            // 2. GUARDAR EN CACHÉ (La llave es el JTI, el valor es el ID del paciente)
            // Le damos el mismo tiempo de vida que tiene el JWT (24 horas)
            _cache.Set(jti, id, TimeSpan.FromHours(24));

            string frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
            string inviteUrl = $"{frontendUrl}/register/{token}";

            return Ok(new { url = inviteUrl });
        }

        /// <summary>
        /// Returns the patient's basic information based on the invitation token. This endpoint is used by the frontend to pre-fill the registration form when the patient clicks on the Magic Link. It validates the token and retrieves the associated patient data without requiring authentication, as the token itself serves as a temporary access key.
        /// </summary>
        [HttpGet("invite-details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPatientByInviteToken([FromHeader(Name = "X-Invite-Token")] string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest(new { message = "Token is missing." });

            string jwtKey = _configuration["Jwt:Key"];
            string issuer = _configuration["Jwt:Issuer"];

            var (patientId, jti) = _authProcess.ValidatePatientInvitationToken(token, jwtKey, issuer);

            // 3. VALIDACIÓN DE CACHÉ: Revisamos si el JTI existe en la memoria
            // Si el servidor se reinició, o el link ya se usó, TryGetValue será falso.
            if (patientId == null || jti == null || !_cache.TryGetValue(jti, out int cachedPatientId) || cachedPatientId != patientId)
            {
                return Unauthorized(new { message = "This link has been deactivated, already used, or expired." });
            }

            var patient = await _dbContext.DBPatients
                .Include(p => p.Insurances)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null) return NotFound(new { message = "Patient record not found." });

            var primaryInsurance = patient.Insurances?.FirstOrDefault();

            return Ok(new
            {
                personalInfo = new { patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Phone, patient.Email },
                insuranceInfo = primaryInsurance != null ? new { primaryInsurance.PayerName, primaryInsurance.MemberId, primaryInsurance.PlanType, primaryInsurance.RelationshipToSubscriber } : null
            });
        }

        /// <summary>
        /// Completes the patient registration by accepting personal details and insurance information. It validates the invitation token, updates the patient's profile with the provided information, and creates an insurance record linked to the patient. The endpoint uses a transaction to ensure that all operations succeed or fail together, maintaining data integrity. It also handles various error scenarios, such as invalid tokens, already completed registrations, and unexpected exceptions, providing appropriate responses for each case.
        /// </summary>
        [HttpPut("complete")]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteRegistration([FromHeader(Name = "X-Invite-Token")] string token, [FromBody] CompleteRegistrationRequestRecord request)
        {
            if (string.IsNullOrEmpty(token)) return Unauthorized(new { message = "Token missing." });

            string jwtKey = _configuration["Jwt:Key"];
            string issuer = _configuration["Jwt:Issuer"];

            var (patientId, jti) = _authProcess.ValidatePatientInvitationToken(token, jwtKey, issuer);
            if (patientId == null || jti == null || !_cache.TryGetValue(jti, out int cachedPatientId) || cachedPatientId != patientId)
            {
                return Unauthorized(new { message = "This link has been deactivated, already used, or expired." });
            }

            var patientEntity = await _dbContext.DBPatients
                .Include(p => p.Insurances)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patientEntity == null) return NotFound(new { message = "Patient not found." });

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _patientProcess.UpdateDetails(
                    patientEntity,
                    request.PersonalInfo.FirstName,
                    request.PersonalInfo.LastName,
                    request.PersonalInfo.DateOfBirth,
                    request.PersonalInfo.Phone,
                    request.PersonalInfo.Email
                );

                var validatedInsurance = _insuranceProcess.CreateInsurance(
                    request.InsuranceInfo.PayerName,
                    request.InsuranceInfo.MemberId,
                    request.InsuranceInfo.PlanType,
                    request.InsuranceInfo.RelationshipToSubscriber
                );

                if (patientEntity.Insurances == null)
                {
                    patientEntity.Insurances = new List<Insurance>();
                }

                
                var existingInsurance = patientEntity.Insurances.FirstOrDefault();
                if (existingInsurance != null)
                {
                    existingInsurance.PayerName = validatedInsurance.PayerName;
                    existingInsurance.MemberId = validatedInsurance.MemberId;
                    existingInsurance.PlanType = validatedInsurance.PlanType;
                    existingInsurance.RelationshipToSubscriber = validatedInsurance.RelationshipToSubscriber;
                }
                else
                {
                    patientEntity.Insurances.Add(validatedInsurance);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _cache.Remove(jti);

                return Ok(new { message = "Registration completed successfully." });
            }
            catch (DomainException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "An unexpected error occurred during registration." });
            }
        }
    }
}