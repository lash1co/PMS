using Domain.Entities;

namespace WebServices.SharedBusiness
{
    /// <summary>
    /// Data Transfer Object representing a comprehensive summary of a clinical encounter.
    /// Includes patient details, clinical notes (SOAP format), record counts, and detailed lists of associated medical records.
    /// </summary>
    /// <param name="Id">The unique identifier of the clinical encounter.</param>
    /// <param name="AppointmentId">The unique identifier of the associated appointment, if applicable.</param>
    /// <param name="Status">The current workflow status of the encounter (e.g., Planned, InProgress, Completed).</param>
    /// <param name="PatientName">The full name of the patient associated with the encounter.</param>
    /// <param name="DoctorName">The name of the doctor associated with the encounter.</param>
    /// <param name="Subjective">The subjective section of the clinical note (patient's reported symptoms and history).</param>
    /// <param name="Objective">The objective section of the clinical note (doctor's physical exam findings and vital signs).</param>
    /// <param name="Assessment">The assessment section of the clinical note (medical diagnosis or problem synthesis).</param>
    /// <param name="Plan">The plan section of the clinical note (treatment, medication, or follow-up instructions).</param>
    /// <param name="ObservationsCount">The total number of clinical observations (e.g., vital signs) recorded during the encounter.</param>
    /// <param name="ConditionsCount">The total number of conditions (diagnoses) recorded during the encounter.</param>
    /// <param name="ProceduresCount">The total number of procedures performed during the encounter.</param>
    /// <param name="AllergiesCount">The total number of allergies or intolerances recorded during the encounter.</param>
    /// <param name="PrescriptionsCount">The total number of prescriptions issued during the encounter.</param>
    /// <param name="Observations">A collection of detailed clinical observations associated with the encounter.</param>
    /// <param name="Allergies">A collection of detailed allergy and intolerance records associated with the encounter.</param>
    /// <param name="Conditions">A collection of detailed medical conditions associated with the encounter.</param>
    /// <param name="Procedures">A collection of detailed medical procedures associated with the encounter.</param>
    public record EncounterSummaryDto(
        int Id,
        int? AppointmentId,
        string Status,
        string PatientName,
        string DoctorName,
        string? Subjective,
        string? Objective,
        string? Assessment,
        string? Plan,
        int ObservationsCount,
        int ConditionsCount,
        int ProceduresCount,
        int AllergiesCount,
        int PrescriptionsCount,
        IEnumerable<object> Observations,
        IEnumerable<object> Allergies,
        IEnumerable<object> Conditions,
        IEnumerable<object> Procedures
    );

    /// <summary>
    /// Data Transfer Object used to update the clinical narrative note (SOAP structure) of an encounter.
    /// </summary>
    /// <param name="Subjective">The patient's chief complaint and history of present illness.</param>
    /// <param name="Objective">The measurable, observable data from the physical examination.</param>
    /// <param name="Assessment">The medical diagnosis or appraisal of the patient's condition.</param>
    /// <param name="Plan">The details regarding treatment, medications, education, and follow-up.</param>
    public record UpdateClinicalNoteRequest(
        string? Subjective,
        string? Objective,
        string? Assessment,
        string? Plan
    );

    /// <summary>
    /// Data Transfer Object used to create a new clinical observation.
    /// </summary>
    /// <param name="Category">The broad classification of the observation (e.g., "vital-signs").</param>
    /// <param name="DisplayName">The human-readable name of the observation (e.g., "Blood Pressure").</param>
    /// <param name="ValueString">The recorded value of the observation as a string (e.g., "120/80").</param>
    /// <param name="Unit">The unit of measurement for the recorded value (e.g., "mmHg").</param>
    public record CreateObservationDto(
        string Category,
        string DisplayName,
        string ValueString,
        string Unit
    );

    /// <summary>
    /// Data Transfer Object used to create a new allergy or intolerance record.
    /// </summary>
    /// <param name="Substance">The specific substance causing the allergy (e.g., "Penicillin", "Peanuts").</param>
    /// <param name="Criticality">The severity or potential risk of the allergy (e.g., "Low", "High", "UnableToAssess").</param>
    /// <param name="Reaction">A description of the adverse reaction caused by the substance (e.g., "Hives", "Anaphylaxis").</param>
    public record CreateAllergyDto(
        string Substance,
        string Criticality,
        string Reaction
    );

    /// <summary>
    /// Data Transfer Object used to create a new medical condition or diagnosis.
    /// </summary>
    /// <param name="Code">The standardized medical code for the condition (e.g., ICD-10 or SNOMED CT code).</param>
    /// <param name="DisplayName">The human-readable name of the condition (e.g., "Type 2 Diabetes Mellitus").</param>
    /// <param name="ClinicalStatus">The current status of the condition (e.g., "Active", "Resolved", "Inactive").</param>
    public record CreateConditionDto(
        string Code,
        string DisplayName,
        string ClinicalStatus
    );

    /// <summary>
    /// Data Transfer Object used to create a new medical procedure record.
    /// </summary>
    /// <param name="Code">The standardized medical code for the procedure (e.g., CPT or SNOMED CT code).</param>
    /// <param name="DisplayName">The human-readable name of the procedure (e.g., "Appendectomy").</param>
    /// <param name="Status">The current completion status of the procedure (e.g., "Completed", "InProgress", "Aborted").</param>
    public record CreateProcedureDto(
        string Code,
        string DisplayName,
        ProcedureStatus Status
    );

    /// <summary>
    /// Request to create an encounter without a previous appointment (Walk-in/Emergency).
    /// </summary>
    public record CreateWalkInRequest(int PatientId, int DoctorId, string? InitialReason);

    /// <summary>
    /// Request to invalidate an encounter (e.g., entered in error).
    /// </summary>
    public record InvalidateEncounterRequest(string Reason);

    /// <summary>
    /// Data Transfer Object used to filter the encounter history.
    /// </summary>
    /// <param name="StartDate">The start date of the filter range.</param>
    /// <param name="EndDate">The end date of the filter range.</param>
    /// <param name="PatientIds">A collection of selected patient identifiers for soft filtering.</param>
    /// <param name="DoctorIds">A collection of selected doctor identifiers for soft filtering.</param>
    /// <param name="EncounterType">Filter by type: 'With Appointment' or 'Emergency'. Null means all.</param>
    public record EncounterHistoryFilterDto(
        DateTime StartDate,
        DateTime EndDate,
        List<int>? PatientIds,
        List<int>? DoctorIds,
        string? EncounterType
    );

    /// <summary>
    /// Data Transfer Object representing a single read-only record in the encounter history.
    /// </summary>
    /// <param name="EncounterId">The unique identifier of the encounter.</param>
    /// <param name="PatientName">The concatenated full name of the patient.</param>
    /// <param name="EncounterDate">The date and time the encounter was completed or started.</param>
    /// <param name="EncounterType">The classification of the encounter ("With Appointment" or "Emergency").</param>
    /// <param name="Reason">The medical reason or chief complaint for the encounter.</param>
    /// <param name="Score">The calculated relevance score based on filter matches.</param>
    public record EncounterHistoryResponseDto(
        int EncounterId,
        string PatientName,
        DateTime EncounterDate,
        string EncounterType,
        string Reason,
        int Score
    );

    /// <summary>
    /// Data Transfer Object representing the nested appointment details for an encounter.
    /// </summary>
    public record AppointmentDetailDto(
        int AppointmentId,
        DateTime StartTime,
        DateTime EndTime,
        string Reason,
        string Status
    );

    /// <summary>
    /// Data Transfer Object representing the comprehensive read-only details of a historical encounter.
    /// </summary>
    public record EncounterHistoryDetailDto(
        int EncounterId,
        string PatientName,
        string DoctorName,
        DateTime StartTime,
        DateTime? EndTime,
        string Status,
        string? StatusReason,
        string? UpdatedBy,
        AppointmentDetailDto? Appointment,
        List<ClinicalNoteDto> Notes,
        List<ClinicalObservationDto> Vitals,
        List<ConditionDto> Diagnoses
    );

    /// <summary>
    /// Data Transfer Object representing a single clinical note entry.
    /// </summary>
    /// <param name="NoteText"></param>
    /// <param name="Date"></param>
    public record ClinicalNoteDto(string NoteText, DateTime Date);
    /// <summary>
    /// Data Transfer Object representing a single clinical observation entry.
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="Value"></param>
    /// <param name="Unit"></param>
    /// <param name="Date"></param>
    public record ClinicalObservationDto(string Type, decimal Value, string Unit, DateTime Date);
    /// <summary>
    /// Data Transfer Object representing a single medical condition entry.
    /// </summary>
    /// <param name="Diagnosis"></param>
    /// <param name="Status"></param>
    /// <param name="Date"></param>
    public record ConditionDto(string Diagnosis, string Status, DateTime Date);
}