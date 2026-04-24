using Domain.Entities;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;

namespace WebServices.SharedBusiness
{
    /// <summary>
    /// Handles the business logic and database queries for schedules and appointments.
    /// </summary>
    public class ScheduleProcess
    {
        private readonly DatabaseContext _dbContext;

        public ScheduleProcess(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a filtered list of schedule views based on the provided filter criteria.
        /// </summary>
        public async Task<List<ScheduleView>> GetFilteredAppointmentsAsync(ScheduleFilter filter)
        {
            var appointments = await _dbContext.DBScheduleViews
                .Where(sv => filter.DoctorsId.Contains(sv.DoctorId) &&
                             (!filter.Date.HasValue || sv.Date == filter.Date.Value) &&
                             (string.IsNullOrEmpty(filter.ScheduleStatus) || sv.ScheduleStatus == filter.ScheduleStatus) &&
                             (string.IsNullOrEmpty(filter.PatientName) || sv.PatientName!.Contains(filter.PatientName)))
                .ToListAsync();

            if (filter.Date.HasValue)
            {
                var targetDate = filter.Date.Value;

                var recurringRests = await _dbContext.DBDoctorRestSchedules
                    .Where(dr => filter.DoctorsId.Contains(dr.DoctorId))
                    .Include(dr => dr.Doctor) 
                    .ToListAsync();

                foreach (var rest in recurringRests)
                {
                    appointments.Add(new ScheduleView
                    {
                        Type = "Rest",
                        Id = rest.Id,
                        Date = targetDate,
                        StartTime = targetDate.ToDateTime(TimeOnly.FromTimeSpan(rest.StartTime)),
                        EndTime = targetDate.ToDateTime(TimeOnly.FromTimeSpan(rest.EndTime)),
                        ScheduleStatus = "",
                        ScheduleDescription = rest.Reason,
                        DoctorId = rest.DoctorId,
                        DoctorName = rest.Doctor?.Name ?? "Doctor",
                        PatientId = null,
                        PatientName = null
                    });
                }
            }

            return appointments.OrderBy(a => a.StartTime).ToList();
        }

        /// <summary>
        /// Retrieves the details of a specific medical appointment.
        /// </summary>
        public async Task<ScheduleView?> GetAppointmentDetailsAsync(int id)
        {
            return await _dbContext.DBScheduleViews
                .FirstOrDefaultAsync(sv => sv.Type == "Appointment" && sv.Id == id);
        }

        /// <summary>
        /// Retrieves the details of a specific doctor's rest period.
        /// </summary>
        public async Task<ScheduleView?> GetRestDetailsAsync(int id)
        {
            return await _dbContext.DBScheduleViews
                .FirstOrDefaultAsync(sv => sv.Type == "Rest" && sv.Id == id);
        }

        /// <summary>
        /// Validates if a proposed appointment time is available for a specific doctor.
        /// It checks for overlaps with existing appointments and recurring rest schedules.
        /// </summary>
        public async Task<(bool IsAvailable, string ErrorMessage)> IsTimeSlotAvailableAsync(int doctorId, DateTime proposedStart, DateTime proposedEnd)
        {
            if (proposedStart >= proposedEnd)
                return (false, "Start time must be earlier than end time.");

            var timeStart = TimeOnly.FromDateTime(proposedStart);
            var timeEnd = TimeOnly.FromDateTime(proposedEnd);

            var doctorRests = await _dbContext.DBDoctorRestSchedules
                .Where(dr => dr.DoctorId == doctorId)
                .ToListAsync();

            foreach (var rest in doctorRests)
            {
                var restStart = TimeOnly.FromTimeSpan(rest.StartTime);
                var restEnd = TimeOnly.FromTimeSpan(rest.EndTime);

                // Lógica de solapamiento de rangos de tiempo
                if (timeStart < restEnd && restStart < timeEnd)
                {
                    return (false, $"The proposed time conflicts with the doctor's rest period ({restStart} to {restEnd}).");
                }
            }

            // 2. Check if it conflicts with another medical appointment on the same day
            var proposedDate = proposedStart.Date;

            var overlappingAppointments = await _dbContext.DBAppointments
                .Where(a => EF.Property<int>(a, "DoctorId") == doctorId && a.StartTime.Date == proposedDate)
                .ToListAsync();

            foreach (var app in overlappingAppointments)
            {
                if (proposedStart < app.EndTime && app.StartTime < proposedEnd)
                {
                    return (false, "The proposed time conflicts with another scheduled medical appointment.");
                }
            }

            return (true, string.Empty);
        }
    }
}