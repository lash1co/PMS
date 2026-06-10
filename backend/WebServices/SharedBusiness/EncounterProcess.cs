using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;

namespace WebServices.SharedBusiness
{
    /// <summary>
    /// Handles the business logic and database operations related to clinical encounters.
    /// This includes starting, completing, and managing clinical records (observations, conditions, procedures, allergies) within an encounter.
    /// </summary>
    public class EncounterProcess
    {
        private readonly DatabaseContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterProcess"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used for data access.</param>
        public EncounterProcess(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        private async Task<Encounter> GetEditableEncounterAsync(int encounterId, int doctorId)
        {
            var encounter = await _dbContext.Encounters.FindAsync(encounterId);
            if (encounter == null) throw new KeyNotFoundException("Encounter not found.");

            if (encounter.DoctorId != doctorId)
            {
                throw new UnauthorizedAccessException("Only the assigned doctor can modify this encounter.");
            }

            if (encounter.Status != EncounterStatus.InProgress)
            {
                throw new InvalidOperationException("Only in-progress encounters can be modified.");
            }

            return encounter;
        }

        /// <summary>
        /// Starts a new clinical encounter based on an existing appointment.
        /// Updates the appointment status to completed and initializes an empty clinical note.
        /// </summary>
        /// <param name="appointmentId">The unique identifier of the appointment.</param>
        /// <returns>A summary of the newly created encounter.</returns>
        /// <exception cref="Exception">Thrown when the appointment is not found.</exception>
        public async Task<EncounterSummaryDto> StartEncounterAsync(int appointmentId, string username, int doctorId)
        {
            var appointment = await _dbContext.DBAppointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) throw new Exception("Appointment not found.");
            if (appointment.Doctor.Id != doctorId)
            {
                throw new UnauthorizedAccessException("Only the assigned doctor can start this encounter.");
            }

            appointment.Status = AppointmentStatus.Completed;

            var encounter = new Encounter
            {
                AppointmentId = appointment.Id,
                DoctorId = appointment.Doctor.Id,
                PatientId = appointment.Patient.Id,
                Status = EncounterStatus.InProgress,
                StartTime = DateTime.UtcNow,
                UpdatedBy = username
            };

            _dbContext.Encounters.Add(encounter);
            await _dbContext.SaveChangesAsync();

            var clinicalNote = new ClinicalNote { EncounterId = encounter.Id, CreatedAt = DateTime.UtcNow };
            _dbContext.ClinicalNotes.Add(clinicalNote);
            await _dbContext.SaveChangesAsync();

            return await GetEncounterSummaryAsync(encounter.Id);
        }

        /// <summary>
        /// Creates an encounter directly without an appointment (Emergency/Walk-in).
        /// </summary>
        public async Task<EncounterSummaryDto> CreateWalkInEncounterAsync(CreateWalkInRequest request, string username, int doctorId)
        {
            if (request.DoctorId != doctorId)
            {
                throw new UnauthorizedAccessException("Doctors can only create encounters assigned to themselves.");
            }

            var encounter = new Encounter
            {
                PatientId = request.PatientId,
                DoctorId = doctorId,
                Status = EncounterStatus.InProgress,
                StartTime = DateTime.UtcNow,
                StatusReason = request.InitialReason,
                UpdatedBy = username
            };

            _dbContext.Encounters.Add(encounter);
            await _dbContext.SaveChangesAsync();

            var clinicalNote = new ClinicalNote { EncounterId = encounter.Id, CreatedAt = DateTime.UtcNow };
            _dbContext.ClinicalNotes.Add(clinicalNote);
            await _dbContext.SaveChangesAsync();

            return await GetEncounterSummaryAsync(encounter.Id);
        }

        /// <summary>
        /// Retrieves a comprehensive summary of a specific encounter, including its associated patient, clinical notes, and medical records.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <returns>A Data Transfer Object (DTO) containing the summarized encounter details.</returns>
        /// <exception cref="Exception">Thrown when the encounter is not found.</exception>
        public async Task<EncounterSummaryDto> GetEncounterSummaryAsync(int encounterId)
        {
            var encounter = await _dbContext.Encounters
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .Include(e => e.ClinicalNote)
                .Include(e => e.Observations)
                .Include(e => e.Conditions)
                .Include(e => e.Procedures)
                .Include(e => e.Allergies)
                .Include(e => e.Prescriptions)
                .Include(e => e.Laboratories)
                    .ThenInclude(l => l.LaboratoriesDetails)
                        .ThenInclude(d => d.Laboratory)
                .FirstOrDefaultAsync(e => e.Id == encounterId);

            if (encounter == null) throw new Exception("Encounter not found.");

            string patientName = encounter.Patient != null
                ? $"{encounter.Patient.LastName} {encounter.Patient.FirstName}"
                : string.Empty;

            string doctorName = encounter.Doctor != null
                ? encounter.Doctor.Name
                : "Unassigned";

            // We explicitly cast to (object) so the compiler matches the right side of the ?? operator
            var observationsList = encounter.Observations?
                .Select(o => (object)new { o.Id, o.Category, o.DisplayName, o.ValueString, o.Unit })
                ?? new List<object>();

            var allergiesList = encounter.Allergies?
                .Select(a => (object)new { a.Id, a.Substance, a.Criticality, a.Reaction })
                ?? new List<object>();

            var conditionsList = encounter.Conditions?
                .Select(c => (object)new { c.Id, c.Code, c.DisplayName, c.ClinicalStatus })
                ?? new List<object>();

            var proceduresList = encounter.Procedures?
                .Select(p => (object)new { p.Id, p.Code, p.DisplayName, p.Status })
                ?? new List<object>();

            var laboratoriesList = encounter.Laboratories?
                .SelectMany(l => l.LaboratoriesDetails?
                    .Where(d => d.Laboratory != null)
                    .Select(d => (object)new
                    {
                        RequestId = l.Id,
                        DetailId = d.Id,
                        LaboratoryId = d.Laboratory!.Id,
                        d.Laboratory.Description,
                        d.Laboratory.Price,
                        d.Laboratory.TimeToCompleteInHours,
                        d.Laboratory.NoFoodBeforeExecuted,
                        d.Laboratory.LiquidIngestionBeforeExecuted,
                        DateOrdered = l.DateOrdered,
                        Status = l.LaboratoryStatus.ToString()
                    }) ?? new List<object>())
                ?? new List<object>();

            return new EncounterSummaryDto(
                encounter.Id,
                encounter.AppointmentId,
                encounter.Status.ToString(),
                patientName,
                doctorName,
                encounter.ClinicalNote?.Subjective,
                encounter.ClinicalNote?.Objective,
                encounter.ClinicalNote?.Assessment,
                encounter.ClinicalNote?.Plan,
                encounter.Observations?.Count ?? 0,
                encounter.Conditions?.Count ?? 0,
                encounter.Procedures?.Count ?? 0,
                encounter.Allergies?.Count ?? 0,
                encounter.Prescriptions?.Count ?? 0,
                laboratoriesList.Count(),
                observationsList,
                allergiesList,
                conditionsList,
                proceduresList,
                laboratoriesList
            );
        }

        /// <summary>
        /// Updates the clinical note (e.g., SOAP note sections) for a given encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="request">The data containing the updated clinical note sections.</param>
        /// <returns>True if the update was successful.</returns>
        /// <exception cref="Exception">Thrown when the clinical note is not found.</exception>
        public async Task<bool> UpdateClinicalNoteAsync(int encounterId, UpdateClinicalNoteRequest request, int doctorId)
        {
            await GetEditableEncounterAsync(encounterId, doctorId);

            var note = await _dbContext.ClinicalNotes.FirstOrDefaultAsync(n => n.EncounterId == encounterId);
            if (note == null) throw new Exception("Clinical Note not found.");

            note.Subjective = request.Subjective;
            note.Objective = request.Objective;
            note.Assessment = request.Assessment;
            note.Plan = request.Plan;
            note.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Marks the specified clinical encounter and its associated appointment as completed.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter to complete.</param>
        /// <returns>True if the encounter was successfully marked as completed.</returns>
        /// <exception cref="Exception">Thrown when the encounter is not found.</exception>
      public async Task<bool> CompleteEncounterAsync(int encounterId, string username, int doctorId)
        {
            var encounter = await _dbContext.Encounters
                .Include(e => e.Appointment)
                .FirstOrDefaultAsync(e => e.Id == encounterId);

            if (encounter == null) throw new Exception("Encounter not found.");
            if (encounter.DoctorId != doctorId)
            {
                throw new UnauthorizedAccessException("Only the assigned doctor can modify this encounter.");
            }
            if (encounter.Status != EncounterStatus.InProgress)
            {
                throw new InvalidOperationException("Only in-progress encounters can be completed.");
            }

            encounter.Status = EncounterStatus.Completed;
            encounter.EndTime = DateTime.UtcNow;
            encounter.UpdatedBy = username; // 👈 Track who completed it

            if (encounter.Appointment != null)
            {
                encounter.Appointment.Status = AppointmentStatus.Completed;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Sets an encounter status to 'Cancelled' to invalidate it without deleting data.
        /// </summary>
        public async Task<bool> InvalidateEncounterAsync(int encounterId, InvalidateEncounterRequest request, string username, int doctorId)
        {
            var encounter = await GetEditableEncounterAsync(encounterId, doctorId);

            encounter.Status = EncounterStatus.Cancelled;
            encounter.StatusReason = request.Reason;
            encounter.UpdatedBy = username;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Adds a new clinical observation (e.g., vital signs) to the specified encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="dto">The data transfer object containing the observation details.</param>
        /// <returns>True if the observation was successfully added; otherwise, false.</returns>
        public async Task<bool> AddObservationAsync(int encounterId, CreateObservationDto dto, int doctorId)
        {
            var encounter = await GetEditableEncounterAsync(encounterId, doctorId);

            _dbContext.ClinicalObservations.Add(new ClinicalObservation
            {
                EncounterId = encounterId,
                PatientId = encounter.PatientId,
                Category = dto.Category,
                Code = "OBS",
                DisplayName = dto.DisplayName,
                ValueString = dto.ValueString,
                Unit = dto.Unit,
                EffectiveDate = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a specific clinical observation from an encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="observationId">The unique identifier of the observation to remove.</param>
        /// <returns>True if the observation was successfully removed; otherwise, false.</returns>
        public async Task<bool> RemoveObservationAsync(int encounterId, int observationId, int doctorId)
        {
            await GetEditableEncounterAsync(encounterId, doctorId);

            var obs = await _dbContext.ClinicalObservations.FirstOrDefaultAsync(o => o.Id == observationId && o.EncounterId == encounterId);
            if (obs == null) return false;

            _dbContext.ClinicalObservations.Remove(obs);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Adds a new medical condition (diagnosis) to the specified encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="dto">The data transfer object containing the condition details.</param>
        /// <returns>True if the condition was successfully added; otherwise, false.</returns>
        public async Task<bool> AddConditionAsync(int encounterId, CreateConditionDto dto, int doctorId)
        {
            var encounter = await GetEditableEncounterAsync(encounterId, doctorId);

            _dbContext.Conditions.Add(new Condition
            {
                EncounterId = encounterId,
                PatientId = encounter.PatientId,
                Code = dto.Code,
                DisplayName = dto.DisplayName,
                RecordedDate = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a specific medical condition from an encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="conditionId">The unique identifier of the condition to remove.</param>
        /// <returns>True if the condition was successfully removed; otherwise, false.</returns>
        public async Task<bool> RemoveConditionAsync(int encounterId, int conditionId, int doctorId)
        {
            await GetEditableEncounterAsync(encounterId, doctorId);

            var condition = await _dbContext.Conditions.FirstOrDefaultAsync(c => c.Id == conditionId && c.EncounterId == encounterId);
            if (condition == null) return false;

            _dbContext.Conditions.Remove(condition);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Adds a new medical procedure to the specified encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="dto">The data transfer object containing the procedure details.</param>
        /// <returns>True if the procedure was successfully added; otherwise, false.</returns>
        public async Task<bool> AddProcedureAsync(int encounterId, CreateProcedureDto dto, int doctorId)
        {
            var encounter = await GetEditableEncounterAsync(encounterId, doctorId);

            _dbContext.Procedures.Add(new Procedure
            {
                EncounterId = encounterId,
                PatientId = encounter.PatientId,
                Code = dto.Code,
                Status = dto.Status,
                DisplayName = dto.DisplayName,
                PerformedDate = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a specific medical procedure from an encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="procedureId">The unique identifier of the procedure to remove.</param>
        /// <returns>True if the procedure was successfully removed; otherwise, false.</returns>
        public async Task<bool> RemoveProcedureAsync(int encounterId, int procedureId, int doctorId)
        {
            await GetEditableEncounterAsync(encounterId, doctorId);

            var procedure = await _dbContext.Procedures.FirstOrDefaultAsync(p => p.Id == procedureId && p.EncounterId == encounterId);
            if (procedure == null) return false;

            _dbContext.Procedures.Remove(procedure);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Adds a new allergy or intolerance record to the specified encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="dto">The data transfer object containing the allergy details.</param>
        /// <returns>True if the allergy was successfully added; otherwise, false.</returns>
        public async Task<bool> AddAllergyAsync(int encounterId, CreateAllergyDto dto, int doctorId)
        {
            var encounter = await GetEditableEncounterAsync(encounterId, doctorId);

            var criticalityEnum = Enum.Parse<AllergyCriticality>(dto.Criticality, true);

            _dbContext.AllergyIntolerances.Add(new AllergyIntolerance
            {
                EncounterId = encounterId,
                PatientId = encounter.PatientId,
                Substance = dto.Substance,
                Criticality = criticalityEnum,
                Reaction = dto.Reaction,
                RecordedDate = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Removes a specific allergy or intolerance record from an encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="allergyId">The unique identifier of the allergy to remove.</param>
        /// <returns>True if the allergy was successfully removed; otherwise, false.</returns>
        public async Task<bool> RemoveAllergyAsync(int encounterId, int allergyId, int doctorId)
        {
            await GetEditableEncounterAsync(encounterId, doctorId);

            var allergy = await _dbContext.AllergyIntolerances.FirstOrDefaultAsync(a => a.Id == allergyId && a.EncounterId == encounterId);
            if (allergy == null) return false;

            _dbContext.AllergyIntolerances.Remove(allergy);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a read-only historical list of completed encounters based on date and soft-match filters.
        /// </summary>
        public async Task<List<EncounterHistoryResponseDto>> GetEncounterHistoryAsync(EncounterHistoryFilterDto filter)
        {
            // FIX: Para evitar el error "Constant expression in ORDER BY", 
            // inyectamos un ID ficticio si las listas vienen vacías o nulas.
            // Esto obliga a EF Core a generar una expresión SQL dinámica válida.
            var pIds = filter.PatientIds != null && filter.PatientIds.Any() ? filter.PatientIds : new List<int> { -999999 };
            var dIds = filter.DoctorIds != null && filter.DoctorIds.Any() ? filter.DoctorIds : new List<int> { -999999 };

            // 1. Base Query with Strict Date and Status Filters
            var query = _dbContext.Encounters.AsNoTracking()
                .Where(e => e.Status == EncounterStatus.Completed || e.Status == EncounterStatus.Cancelled)
                .Where(e => e.StartTime >= filter.StartDate && e.StartTime <= filter.EndDate);

            // 2. Dynamic Encounter Type Filter
            if (!string.IsNullOrEmpty(filter.EncounterType))
            {
                if (filter.EncounterType == "With Appointment")
                    query = query.Where(e => e.AppointmentId != null);
                else if (filter.EncounterType == "Emergency")
                    query = query.Where(e => e.AppointmentId == null);
            }

            // 3. Projection to an Anonymous Type (100% translatable to SQL Server)
            var dbQuery = query.Select(e => new
            {
                e.Id,
                PatientName = e.Patient.LastName + " " + e.Patient.FirstName,
                EncounterDate = e.StartTime,
                HasAppointment = e.AppointmentId != null,
                Reason = e.AppointmentId != null ? e.Appointment.Reason : (e.StatusReason ?? "Walk-In"),
                Score = (pIds.Contains(e.PatientId) ? 2 : 0) + (dIds.Contains(e.DoctorId) ? 2 : 0)
            });

            // 4. Sort and execute query against Database
            var results = await dbQuery
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.EncounterDate)
                .ToListAsync();

            // 5. In-Memory mapping to the C# Record DTO
            return results.Select(x => new EncounterHistoryResponseDto(
                x.Id,
                x.PatientName,
                x.EncounterDate,
                x.HasAppointment ? "With Appointment" : "Emergency",
                x.Reason,
                x.Score
            )).ToList();
        }

        /// <summary>
        /// Retrieves comprehensive read-only details for a specific historical encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <returns>A detailed DTO including nested appointment information if applicable.</returns>
        public async Task<EncounterHistoryDetailDto> GetEncounterHistoryDetailAsync(int encounterId)
        {
            var rawData = await _dbContext.Encounters.AsNoTracking()
                .Where(e => e.Id == encounterId)
                .Select(e => new
                {
                    e.Id,
                    PatientName = e.Patient.LastName + " " + e.Patient.FirstName,
                    DoctorName = e.Doctor.Name,
                    e.StartTime,
                    e.EndTime,
                    Status = e.Status.ToString(),
                    e.StatusReason,
                    e.UpdatedBy,
                    Appointment = e.Appointment,
                    ClinicalNote = e.ClinicalNote,
                    Observations = e.Observations,
                    Conditions = e.Conditions
                })
                .FirstOrDefaultAsync();

            if (rawData == null)
                throw new KeyNotFoundException("Historical encounter not found.");

            var detailDto = new EncounterHistoryDetailDto(
                rawData.Id,
                rawData.PatientName,
                rawData.DoctorName,
                rawData.StartTime,
                rawData.EndTime,
                rawData.Status,
                rawData.StatusReason,
                rawData.UpdatedBy,

                rawData.Appointment != null ? new AppointmentDetailDto(
                    rawData.Appointment.Id,
                    rawData.Appointment.StartTime,
                    rawData.Appointment.EndTime,
                    rawData.Appointment.Reason,
                    rawData.Appointment.Status.ToString()
                ) : null,

                rawData.ClinicalNote != null ? new List<ClinicalNoteDto>
                {
                    new ClinicalNoteDto(
                        $"S: {rawData.ClinicalNote.Subjective}\nO: {rawData.ClinicalNote.Objective}\nA: {rawData.ClinicalNote.Assessment}\nP: {rawData.ClinicalNote.Plan}",
                        rawData.ClinicalNote.CreatedAt
                    )
                } : new List<ClinicalNoteDto>(),

                rawData.Observations?.Select(o => new ClinicalObservationDto(
                    o.DisplayName ?? "Measurement",
                    o.ValueQuantity ?? 0m,
                    o.Unit ?? "",
                    o.EffectiveDate
                )).ToList() ?? new List<ClinicalObservationDto>(),

                rawData.Conditions?.Select(c => new ConditionDto(
                    c.DisplayName ?? "Unknown",
                    c.ClinicalStatus.ToString(),
                    c.RecordedDate
                )).ToList() ?? new List<ConditionDto>()
            );

            return detailDto;
        }
          
        public async Task<bool> AddLaboratoryRequestAsync(int encounterId, CreateLaboratoryRequestDto dto, int doctorId)
        {
            var encounter = await GetEditableEncounterAsync(encounterId, doctorId);

            var laboratory = await _dbContext.Laboratories.FindAsync(dto.LaboratoryId);
            if (laboratory == null)
            {
                throw new KeyNotFoundException("Laboratory not found.");
            }

            var encounterLaboratories = await _dbContext.EncounterLaboratories
                .Include(l => l.LaboratoriesDetails)
                    .ThenInclude(d => d.Laboratory)
                .Where(l => l.Encounter != null && l.Encounter.Id == encounterId)
                .ToListAsync();

            var isDuplicate = encounterLaboratories
                .SelectMany(l => l.LaboratoriesDetails ?? new List<EncounterLaboratoriesDetail>())
                .Any(d => d.Laboratory != null && d.Laboratory.Id == dto.LaboratoryId);

            if (isDuplicate)
            {
                throw new InvalidOperationException("This laboratory has already been requested for this encounter.");
            }

            _dbContext.EncounterLaboratories.Add(new EncounterLaboratories
            {
                Encounter = encounter,
                DateOrdered = DateOnly.FromDateTime(DateTime.UtcNow),
                LaboratoryStatus = LaboratoryStatus.Requested,
                LaboratoriesDetails = new List<EncounterLaboratoriesDetail>
                {
                    new EncounterLaboratoriesDetail
                    {
                        Laboratory = laboratory
                    }
                }
            });

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
