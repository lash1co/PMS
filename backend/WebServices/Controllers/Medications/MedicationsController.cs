using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;

namespace WebServices.Controllers.Medications
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationsController : Controller
    {
               
        private readonly DatabaseContext _dbContext;

        public MedicationsController(DatabaseContext dbContext)
            {
                _dbContext = dbContext;
            }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Medication>>> GetAll()
            {
                return await _dbContext.Medications.ToListAsync();
            }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Medication>>> Search([FromQuery] string? searchTerm)
        {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await _dbContext.Medications.ToListAsync();

                return await _dbContext.Medications
                    .Where(m => m.Name.Contains(searchTerm))
                    .ToListAsync();
            }

         [HttpGet("{id}")]
         public async Task<ActionResult<Medication>> GetById(int id)
            {
                var med = await _dbContext.Medications.FindAsync(id);
                if (med == null) return NotFound();
                return med;
            }

         [HttpPost]
         public async Task<ActionResult<Medication>> Create([FromBody] Medication medication)
            {
                _dbContext.Medications.Add(medication);
                await _dbContext.SaveChangesAsync();
                return Ok(medication);
            }

         [HttpPut("{id}")]
         public async Task<IActionResult> Update(int id, [FromBody] Medication medication)
            {
                if (id != medication.Id) return BadRequest();
                _dbContext.Entry(medication).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return Ok(medication);
            }

         [HttpDelete("{id}")]
         public async Task<IActionResult> Delete(int id)
            {
                var med = await _dbContext.Medications.FindAsync(id);
                if (med == null) return NotFound();
                _dbContext.Medications.Remove(med);
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
        
    }
}

