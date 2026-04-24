using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;

namespace WebServices.SharedBusiness
{
    /// <summary>
    /// Handles the business logic and database queries for doctors.
    /// </summary>
    public class DoctorProcess
    {
        private readonly DatabaseContext _dbContext;

        public DoctorProcess(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Searches for doctors by matching text against their name or exact ID.
        /// Ignores accents and case sensitivity.
        /// </summary>
        /// <param name="searchTerm">The ID or Name to search for.</param>
        /// <returns>A list of doctors matching the search criteria.</returns>
        public async Task<List<Doctor>> SearchDoctorsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _dbContext.DBDoctors.ToListAsync();
            }

            string collation = "SQL_Latin1_General_CP1_CI_AI";
            var query = _dbContext.DBDoctors.AsQueryable();

            var terms = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var term in terms)
            {
                query = query.Where(d =>
                    EF.Functions.Collate(d.Name, collation).Contains(term) ||
                    d.Id.ToString() == term);
            }

            return await query.ToListAsync();
        }
    }
}