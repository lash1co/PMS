using Domain.Entities;
using WebServices.DataAccess;

namespace WebServices.Repositories
{
    public class AppointmentsRepository
    {
        private readonly DatabaseContext _dbContext;

        public AppointmentsRepository(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UpsertRequest> CreateAppointment(Appointment appointment)
        {
            try
            {
                // Validate the appointment's time slot availability before proceeding with the creation.
                var process = new WebServices.SharedBusiness.ScheduleProcess(_dbContext);
                var availability = await process.IsTimeSlotAvailableAsync(
                    appointment.Doctor.Id,
                    appointment.StartTime,
                    appointment.EndTime);

                if (!availability.IsAvailable)
                    return new UpsertRequest { UpsertSuccessfull = false, Message = availability.ErrorMessage };

                // Search for the tracked entities in the database context
                var trackedDoctor = await _dbContext.DBDoctors.FindAsync(appointment.Doctor.Id);
                var trackedPatient = await _dbContext.DBPatients.FindAsync(appointment.Patient.Id);

                // Security check: If either the Doctor or Patient does not exist, return an error message.
                if (trackedDoctor == null)
                    return new UpsertRequest { UpsertSuccessfull = false, Message = "The specified Doctor does not exist." };
                if (trackedPatient == null)
                    return new UpsertRequest { UpsertSuccessfull = false, Message = "The specified Patient does not exist." };
                // Assign the real and tracked entities to the new appointment
                appointment.Doctor = trackedDoctor;
                appointment.Patient = trackedPatient;

                _dbContext.DBAppointments.Add(appointment);

                await _dbContext.SaveChangesAsync();

                return new UpsertRequest { UpsertSuccessfull = true, Message = "Appointment created successfully." };
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new UpsertRequest { UpsertSuccessfull = false, Message = errorMsg };
            }
        }
    }
}