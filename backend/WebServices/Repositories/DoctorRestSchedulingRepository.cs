using Domain.Entities;
using WebServices.DataAccess;

namespace WebServices.Repositories
{
    /// <summary>
    /// Provides methods for creating, updating, and deleting doctor rest schedules in the database.
    /// </summary>
    /// <remarks>This repository encapsulates data access logic related to doctor rest scheduling. All
    /// operations are performed asynchronously and return an UpsertRequest indicating the result of the operation. The
    /// repository does not throw exceptions for not found entities; instead, it returns an unsuccessful result with an
    /// explanatory message.</remarks>
    public class DoctorRestSchedulingRepository
    {
        private readonly DatabaseContext _databaseContext;

        public DoctorRestSchedulingRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        /// <summary>
        /// Creates a new doctor rest schedule and saves it to the database.
        /// </summary>
        /// <param name="restSchedule">The doctor rest schedule to be created. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an UpsertRequest indicating
        /// whether the creation was successful and includes a message describing the outcome.</returns>
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

        /// <summary>
        /// Updates an existing doctor's rest schedule in the database asynchronously.
        /// </summary>
        /// <remarks>If the specified rest schedule does not exist in the database, the operation may not
        /// update any records. The returned UpsertRequest provides details about the success or failure of the
        /// operation.</remarks>
        /// <param name="restSchedule">The doctor rest schedule entity to update. Must not be null and should represent an existing schedule to be
        /// modified.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an UpsertRequest indicating
        /// whether the update was successful and includes a message describing the outcome.</returns>
        public async Task<UpsertRequest> UpdateDoctorRestSchedule(DoctorRestSchedule restSchedule)
        {
            try
            {
                _databaseContext.DBDoctorRestSchedules.Update(restSchedule);
                await _databaseContext.SaveChangesAsync();
                return new UpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "Doctor rest schedule updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new UpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error updating doctor rest schedule: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes the doctor rest schedule with the specified identifier.
        /// </summary>
        /// <remarks>If the specified rest schedule is not found, the method returns an unsuccessful
        /// result without throwing an exception. Any errors encountered during the operation are reported in the
        /// returned message.</remarks>
        /// <param name="restScheduleId">The unique identifier of the doctor rest schedule to delete.</param>
        /// <returns>An UpsertRequest indicating whether the deletion was successful. If the specified rest schedule does not
        /// exist, the operation is unsuccessful and an appropriate message is provided.</returns>
        public async Task<UpsertRequest> DeleteDoctorRestSchedule(int restScheduleId)
        {
            try
            {
                var restSchedule = await _databaseContext.DBDoctorRestSchedules.FindAsync(restScheduleId);

                if (restSchedule == null)
                {
                    return new UpsertRequest
                    {
                        UpsertSuccessfull = false,
                        Message = "Doctor rest schedule not found."
                    };
                }

                _databaseContext.DBDoctorRestSchedules.Remove(restSchedule);
                await _databaseContext.SaveChangesAsync();

                return new UpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "Doctor rest schedule deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new UpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error deleting doctor rest schedule: {ex.Message}"
                };
            }
        }
    }
}
