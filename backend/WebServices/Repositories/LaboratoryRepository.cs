using Domain.Entities;
using WebServices.DataAccess;

namespace WebServices.Repositories
{
    /// <summary>
    /// Provides methods for creating, updating, and deleting laboratory records in the database.
    /// </summary>
    /// <remarks>This repository encapsulates data access logic for laboratory entities, enabling asynchronous
    /// operations for modifying laboratory data. It is intended to be used as part of a data access layer and requires
    /// a valid DatabaseContext instance for initialization. All operations return an UpsertRequest indicating the
    /// success or failure of the operation, along with a descriptive message.</remarks>
    public class LaboratoryRepository
    {
        private readonly DatabaseContext _databaseContext;

        public LaboratoryRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        /// <summary>
        /// Creates a new laboratory record in the database asynchronously.
        /// </summary>
        /// <param name="laboratory">The laboratory entity to add to the database. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an UpsertRequest indicating
        /// whether the creation was successful and includes a message describing the outcome.</returns>
        public async Task<UpsertRequest> CreateLaboratory(Laboratory laboratory)
        {
            try
            {
                var laboratoryIndex = _databaseContext.Laboratories
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault()?.Id ?? 0;
                laboratory.Id = laboratoryIndex + 1;
                _databaseContext.Laboratories.Add(laboratory);
                await _databaseContext.SaveChangesAsync();
                return new UpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "Laboratory created successfully."
                };
            }
            catch (Exception ex)
            {
                return new UpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error creating laboratory: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Updates the specified laboratory entity in the database asynchronously.
        /// </summary>
        /// <remarks>If the update fails due to an exception, the returned UpsertRequest will indicate
        /// failure and include the error message. The method does not throw exceptions for update failures; instead, it
        /// reports them in the result object.</remarks>
        /// <param name="laboratory">The laboratory entity to update. Must not be null and should represent an existing laboratory with valid
        /// data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an UpsertRequest indicating
        /// whether the update was successful and includes a message describing the outcome.</returns>
        public async Task<UpsertRequest> UpdateLaboratory(Laboratory laboratory)
        {
            try
            {
                _databaseContext.Laboratories.Update(laboratory);
                await _databaseContext.SaveChangesAsync();
                return new UpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "Laboratory updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new UpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error updating laboratory: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Deletes the laboratory with the specified identifier and returns the result of the operation.
        /// </summary>
        /// <remarks>If the laboratory does not exist, the operation is not performed and the result
        /// indicates failure. The returned message provides additional context about the outcome.</remarks>
        /// <param name="laboratoryId">The unique identifier of the laboratory to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an UpsertRequest indicating
        /// whether the deletion was successful and providing a descriptive message.</returns>
        public async Task<UpsertRequest> DeleteLaboratory(int laboratoryId)
        {
            try
            {
                var laboratory = await _databaseContext.Laboratories.FindAsync(laboratoryId);
                if (laboratory == null)
                {
                    return new UpsertRequest
                    {
                        UpsertSuccessfull = false,
                        Message = "Laboratory not found."
                    };
                }

                _databaseContext.Laboratories.Remove(laboratory);
                await _databaseContext.SaveChangesAsync();

                return new UpsertRequest
                {
                    UpsertSuccessfull = true,
                    Message = "Laboratory deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new UpsertRequest
                {
                    UpsertSuccessfull = false,
                    Message = $"Error deleting Laboratory: {ex.Message}"
                };
            }
        }
    }
}
