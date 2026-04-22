using Domain.Entities;
using WebServices.DataAccess;

namespace WebServices.Repositories
{
    public class AppointmentsRepository
    {
        private readonly DatabaseContext _dbContext;

        public AppointmentsRepository(DataAccess.DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UpsertRequest> CreateAppointment(Appointment appointment)
        {
            try
            {
                _dbContext.DBAppointments.Add(appointment);
                await _dbContext.SaveChangesAsync();
                return new UpsertRequest { UpsertSuccessfull = true, Message = "Appointment created successfully." };
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if needed
                return new UpsertRequest { UpsertSuccessfull = false, Message = $"Error creating appointment: {ex.Message}" };
            }
        }
    }
}
