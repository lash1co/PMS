using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.Repositories;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Scheduling
{
    /// <summary>
    /// Provides RESTful API endpoints for managing doctor rest schedules, including retrieval, creation, updating, and
    /// deletion of rest time entries for doctors.
    /// </summary>
    /// <remarks>All endpoints require authorization and restrict access to users with Admin, Doctor, or
    /// Receptionist roles. The controller validates authorization tokens for each request and interacts with the
    /// underlying database context to perform operations on doctor rest schedules. Use this controller to integrate
    /// doctor rest schedule management into client applications or administrative tools.</remarks>
    public class DoctorRestController : Controller
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _context;
        private readonly List<string> _authorizedRoles = new List<string>
        {
            UserConstants.RoleConstants.AdminRole,
            UserConstants.RoleConstants.DoctorRole,
            UserConstants.RoleConstants.RecepcionistRole
        };

        public DoctorRestController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        // GET: DoctorRestController/GetDoctorRests
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<DoctorRestSchedule>>> GetDoctorRests(int doctorId)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);

            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var doctorRests = await _context.DBDoctorRestSchedules
                .Where(dr => dr.DoctorId == doctorId)
                .ToListAsync();
            return Ok(doctorRests);
        }

        // POST: DoctorRestController/CreateRestTime
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UpsertRequest>> CreateRestTime([FromBody] DoctorRestSchedule doctorRestSchedule)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var doctorRestSchedulingRepository = new DoctorRestSchedulingRepository(_context);
            var createProcessResult = await doctorRestSchedulingRepository.CreateDoctorRestSchedule(doctorRestSchedule);
            if (!createProcessResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    createProcessResult.Message
                );
            }

            return Ok(createProcessResult);
        }

        // PUT: DoctorRestController/UpdateRestTime
        [Authorize]
        [HttpPut]
        public async Task<ActionResult<UpsertRequest>> UpdateRestTime([FromBody] DoctorRestSchedule doctorRestSchedule)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }
            var doctorRestSchedulingRepository = new DoctorRestSchedulingRepository(_context);
            var updateProcessResult = await doctorRestSchedulingRepository.UpdateDoctorRestSchedule(doctorRestSchedule);
            if (!updateProcessResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    updateProcessResult.Message
                );
            }
            return Ok(updateProcessResult);
        }

        // DELETE: DoctorRestController/DeleteRestTime
        [Authorize]
        [HttpDelete]
        public async Task<ActionResult<UpsertRequest>> DeleteRestTime(int restScheduleId)
        {
            var validationProcess = new TokenValidationProcess(_config, _context);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }
            var doctorRestSchedulingRepository = new DoctorRestSchedulingRepository(_context);
            var deleteProcessResult = await doctorRestSchedulingRepository.DeleteDoctorRestSchedule(restScheduleId);
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