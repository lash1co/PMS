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

        /// <summary>
        /// Starts a new clinical encounter based on an existing appointment.
        /// Updates the appointment status to completed and initializes an empty clinical note.
        /// </summary>
        /// <param name="appointmentId">The unique identifier of the appointment.</param>
        /// <returns>A summary of the newly created encounter.</returns>
        /// <exception cref="Exception">Thrown when the appointment is not found.</exception>
        public async Task<EncounterSummaryDto> StartEncounterAsync(int appointmentId, string username)
        {
            var appointment = await _dbContext.DBAppointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) throw new Exception("Appointment not found.");

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
        public async Task<EncounterSummaryDto> CreateWalkInEncounterAsync(CreateWalkInRequest request, string username)
        {
            var encounter = new Encounter
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
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
                observationsList,
                allergiesList,
                conditionsList,
                proceduresList
            );
        }

        /// <summary>
        /// Updates the clinical note (e.g., SOAP note sections) for a given encounter.
        /// </summary>
        /// <param name="encounterId">The unique identifier of the encounter.</param>
        /// <param name="request">The data containing the updated clinical note sections.</param>
        /// <returns>True if the update was successful.</returns>
        /// <exception cref="Exception">Thrown when the clinical note is not found.</exception>
        public async Task<bool> UpdateClinicalNoteAsync(int encounterId, UpdateClinicalNoteRequest request)
        {
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
      public async Task<bool> CompleteEncounterAsync(int encounterId, string username)
        {
            var encounter = await _dbContext.Encounters
                .Include(e => e.Appointment)
                .FirstOrDefaultAsync(e => e.Id == encounterId);

            if (encounter == null) throw new Exception("Encounter not found.");

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
        public async Task<bool> InvalidateEncounterAsync(int encounterId, InvalidateEncounterRequest request, string username)
        {
            var encounter = await _dbContext.Encounters.FindAsync(encounterId);
            if (encounter == null) return false;

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
        public async Task<bool> AddObservationAsync(int encounterId, CreateObservationDto dto)
        {
            var encounter = await _dbContext.Encounters.FindAsync(encounterId);
            if (encounter == null) return false;

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
        public async Task<bool> RemoveObservationAsync(int encounterId, int observationId)
        {
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
        public async Task<bool> AddConditionAsync(int encounterId, CreateConditionDto dto)
        {
            var encounter = await _dbContext.Encounters.FindAsync(encounterId);
            if (encounter == null) return false;

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
        public async Task<bool> RemoveConditionAsync(int encounterId, int conditionId)
        {
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
        public async Task<bool> AddProcedureAsync(int encounterId, CreateProcedureDto dto)
        {
            var encounter = await _dbContext.Encounters.FindAsync(encounterId);
            if (encounter == null) return false;

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
        public async Task<bool> RemoveProcedureAsync(int encounterId, int procedureId)
        {
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
        public async Task<bool> AddAllergyAsync(int encounterId, CreateAllergyDto dto)
        {
            var encounter = await _dbContext.Encounters.FindAsync(encounterId);
            if (encounter == null) return false;

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
        public async Task<bool> RemoveAllergyAsync(int encounterId, int allergyId)
        {
            var allergy = await _dbContext.AllergyIntolerances.FirstOrDefaultAsync(a => a.Id == allergyId && a.EncounterId == encounterId);
            if (allergy == null) return false;

            _dbContext.AllergyIntolerances.Remove(allergy);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}