using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebServices.SharedBusiness.Analytics;
using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.Controllers.Analytics
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin, Management")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsProcess _reportsProcess;

        public ReportsController(IReportsProcess reportsProcess)
        {
            _reportsProcess = reportsProcess;
        }

        /// <summary>
        /// Genera el reporte de cierre operativo y financiero de un mes específico.
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
    }
}