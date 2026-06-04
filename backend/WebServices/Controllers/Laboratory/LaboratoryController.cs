using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.Repositories;
using WebServices.SharedBusiness;
using LabEntity = Domain.Entities.Laboratory;

namespace WebServices.Controllers.Laboratory
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaboratoryController : Controller
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _context;
        private readonly List<string> _authorizedRoles = new List<string>
        {
            UserConstants.RoleConstants.AdminRole
        };

        private readonly List<string> _readAuthorizedRoles = new List<string>
        {
            UserConstants.RoleConstants.AdminRole,
            UserConstants.RoleConstants.DoctorRole
        };

        public LaboratoryController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        [Authorize]
        [HttpGet]        
        public async Task<ActionResult<List<LabEntity>>> GtLaboratories()
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _readAuthorizedRoles);

            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var laboratories = await _context.Laboratories
                .AsNoTracking()
                .ToListAsync();

            return Ok(laboratories);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UpsertRequest>> CreateLaboratory([FromBody] LabEntity laboratory)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var laboratoryRepository = new LaboratoryRepository(_context);
            var createProcessResult = await laboratoryRepository.CreateLaboratory(laboratory);
            if (!createProcessResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    createProcessResult.Message
                );
            }

            return Ok(createProcessResult);
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult<UpsertRequest>> UpdateLaboratory([FromBody] LabEntity laboratory)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var laboratoryRepository = new LaboratoryRepository(_context);
            var updateProcessResult = await laboratoryRepository.UpdateLaboratory(laboratory);
            if (!updateProcessResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    updateProcessResult.Message
                );
            }
            return Ok(updateProcessResult);
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult<UpsertRequest>> DeleteLaboratory(int laboratoryId)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var laboratoryRepository = new LaboratoryRepository(_context);
            var deleteProcessResult = await laboratoryRepository.DeleteLaboratory(laboratoryId);
            if (!deleteProcessResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    deleteProcessResult.Message
                );
            }
            return Ok(deleteProcessResult);
        }
    }
}
