using Domain.Entities;
using WebServices.DataAccess;

namespace WebServices.Repositories
{
    public class DoctorRestSchedulingRepository
    {
        private readonly DatabaseContext _databaseContext;

        public DoctorRestSchedulingRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<UpsertRequest> CreateDoctorRestSchedule(DoctorRestSchedule restSchedule)
        {
            try
            {
                _databaseContext.DBDoctorRestSchedules.Add(restSchedule);
                await _databaseContext.SaveChangesAsync();
                return new UpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "Doctor rest schedule created successfully."
                };
            }
            catch (Exception ex)
            {
                return new UpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error creating doctor rest schedule: {ex.Message}"
                };
            }
        }
    }
}
