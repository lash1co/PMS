using Domain.Entities;
using Domain.Filters;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.Repositories;

namespace WebServices.Controllers.Scheduling
{
    public class AppointmentsController : Controller
    {
        private IConfiguration _config;
        private readonly DatabaseContext _context;
        private List<string> _authorizedRoles = new List<string>
        {
            UserConstants.RoleConstants.AdminRole,
            UserConstants.RoleConstants.DoctorRole,
            UserConstants.RoleConstants.RecepcionistRole
        };

        public AppointmentsController(IConfiguration config, DatabaseContext context)
        {
            _config = config;
            _context = context;
        }

        [Authorize]
        public async Task<ActionResult<List<ScheduleView>>> GetAppointments([FromBody] ScheduleFilter filter)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var validation = new TokenRoleValidator(_context);

            var validationResult = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!validationResult.tokenIsValid)
            {
                return Unauthorized(new
                {
                    Message = validationResult.message
                });
            }

            if (!_authorizedRoles.Contains(validationResult.role))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    validationResult.role + " does not have permission to execute this process."
                );
            }

            return await _context.DBScheduleViews
                .Where(sv => filter.DoctorsId.Contains(sv.DoctorId) &&
                             (!filter.Date.HasValue || sv.Date == filter.Date.Value) &&
                             (string.IsNullOrEmpty(filter.ScheduleStatus) || sv.ScheduleStatus == filter.ScheduleStatus) &&
                             (string.IsNullOrEmpty(filter.PatientName) || sv.PatientName!.Contains(filter.PatientName)))
                .ToListAsync();
        }

        [Authorize]
        // GET: AppointmentsController/AppointmentDetails/5
        public async Task<ActionResult<ScheduleView>> AppointmentDetails(int id)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var validation = new TokenRoleValidator(_context);

            var validationResult = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!validationResult.tokenIsValid)
            {
                return Unauthorized(new
                {
                    Message = validationResult.message
                });
            }

            if (!_authorizedRoles.Contains(validationResult.role))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    validationResult.role + " does not have permission to execute this process."
                );
            }

            return await _context.DBScheduleViews
                .FirstOrDefaultAsync(sv => sv.Type == "Appointment" && sv.Id == id) is ScheduleView scheduleView
                    ? Ok(scheduleView)
                    : NotFound();
        }

        [Authorize]
        // GET: AppointmentsController/RestDetails/5
        public async Task<ActionResult<ScheduleView>> RestDetails(int id)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var validation = new TokenRoleValidator(_context);

            var validationResult = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!validationResult.tokenIsValid)
            {
                return Unauthorized(new
                {
                    Message = validationResult.message
                });
            }

            if (!_authorizedRoles.Contains(validationResult.role))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    validationResult.role + " does not have permission to execute this process."
                );
            }

            return await _context.DBScheduleViews
                .FirstOrDefaultAsync(sv => sv.Type == "Rest" && sv.Id == id) is ScheduleView scheduleView
                    ? Ok(scheduleView)
                    : NotFound();
        }

        // POST: AppointmentsController/Create
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UpsertRequest>> CreateAppointment([FromBody] Appointment appointment)
        {
            var authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized("Invalid authorization token.");
            }

            var validation = new TokenRoleValidator(_context);

            var validationResult = await validation.ValidateTokenAndGetRole(
                authorizationHeader,
                _config["Jwt:Key"],
                _config["Jwt:Issuer"]);

            if (!validationResult.tokenIsValid)
            {
                return Unauthorized(new
                {
                    Message = validationResult.message
                });
            }

            if (!_authorizedRoles.Contains(validationResult.role))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    validationResult.role + " does not have permission to execute this process."
                );
            }

            var appointmentProcessor = new AppointmentsRepository(_context);
            var createProcessResult = await appointmentProcessor.CreateAppointment(appointment);
            if (!createProcessResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    createProcessResult.Message
                );
            }

            return Ok(createProcessResult);
        }

        // GET: AppointmentsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AppointmentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AppointmentsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AppointmentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
