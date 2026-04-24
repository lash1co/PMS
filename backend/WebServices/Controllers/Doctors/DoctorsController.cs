using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Doctors
{
    /// <summary>
    /// API Controller responsible for managing doctors' information.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Descomenta esta línea si decides requerir autenticación para este controlador
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorProcess _doctorProcess;

        public DoctorsController(DatabaseContext dbContext)
        {
            // Inicializamos la capa de proceso de negocio
            _doctorProcess = new DoctorProcess(dbContext);
        }

        /// <summary>
        /// Searches for doctors by matching text against their name or exact ID.
        /// </summary>
        /// <param name="searchTerm">The ID or Name to search for.</param>
        /// <returns>A list of doctors matching the search criteria.</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Doctor>>> SearchDoctors([FromQuery] string searchTerm = "")
        {
            // Toda la lógica de Entity Framework y sanitización se maneja en el Process
            var doctors = await _doctorProcess.SearchDoctorsAsync(searchTerm);
            return Ok(doctors);
        }
    }
}