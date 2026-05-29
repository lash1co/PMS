using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebServices.SharedBusiness.Analytics;
using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsProcess _reportsProcess;

        public ReportsController(IReportsProcess reportsProcess)
        {
            _reportsProcess = reportsProcess;
        }

        /// <summary>
        /// Generate a monthly closing report for a given year and month. The report includes a summary of key financial metrics and productivity indicators for doctors.
        /// </summary>
        [HttpGet("monthly-closing")]
        public async Task<ActionResult<MonthlyClosingReportDto>> GetMonthlyClosingReport([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 2000 || month < 1 || month > 12)
            {
                return BadRequest("Parámetros de fecha inválidos.");
            }

            var report = await _reportsProcess.GenerateMonthlyClosingReportAsync(year, month);
            return Ok(report);
        }

        /// <summary>
        /// Generate a morbidity report for a given year and month.
        /// </summary>
        [HttpGet("morbidity")]
        public async Task<ActionResult<MorbidityReportDto>> GetMorbidityReport([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 2000 || month < 1 || month > 12) return BadRequest("Fecha inválida.");
            return Ok(await _reportsProcess.GenerateMorbidityReportAsync(year, month));
        }

        /// <summary>
        /// Gets the accounts receivable report asynchronously.
        /// </summary>
        /// <remarks>The report is produced asynchronously by the reports processing component.</remarks>
        /// <returns>An ActionResult containing an AccountsReceivableReportDto. Returns 200 OK with the report when successful;
        /// error status codes may be returned for failure scenarios.</returns>
        [HttpGet("accounts-receivable")]
        public async Task<ActionResult<AccountsReceivableReportDto>> GetAccountsReceivableReport()
        {
            return Ok(await _reportsProcess.GenerateAccountsReceivableReportAsync());
        }

        /// <summary>
        /// Gets an appointment efficiency report for the specified year and month.
        /// </summary>
        /// <param name="year">Report year; must be 2000 or later.</param>
        /// <param name="month">Report month, from 1 to 12.</param>
        /// <returns>An ActionResult containing an AppointmentEfficiencyReportDto for the requested period; returns BadRequest if
        /// the year or month is invalid.</returns>
        [HttpGet("appointment-efficiency")]
        public async Task<ActionResult<AppointmentEfficiencyReportDto>> GetAppointmentEfficiencyReport([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 2000 || month < 1 || month > 12) return BadRequest("Fecha inválida.");
            return Ok(await _reportsProcess.GenerateAppointmentEfficiencyReportAsync(year, month));
        }
    }
}