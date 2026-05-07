using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;

namespace WebServices.SharedBusiness
{
    public class MedicationProcess
    {
        private readonly DatabaseContext _context;

        public MedicationProcess(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Medication>> SearchMedicationsAsync(string term)
        {
            term = term?.ToLower() ?? "";

            return await _context.Medications
                .Where(m =>
                    m.Name.ToLower().Contains(term) ||
                    m.Id.ToString() == term
                )
                .ToListAsync();
        }
    }
}