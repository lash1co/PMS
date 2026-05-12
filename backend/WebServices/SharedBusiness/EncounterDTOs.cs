namespace WebServices.SharedBusiness
{
    public record EncounterSummaryDto(
        int Id,
        int? AppointmentId,
        string Status,
        string PatientName, 
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
        IEnumerable<object> Allergies
    );

    public record UpdateClinicalNoteRequest(
        string? Subjective,
        string? Objective,
        string? Assessment,
        string? Plan
    );

    public record CreateObservationDto(string Category, string DisplayName, string ValueString, string Unit);
    public record CreateAllergyDto(string Substance, string Criticality, string Reaction);
    public record CreateConditionDto(string Code, string DisplayName, string ClinicalStatus);
    public record CreateProcedureDto(string Code, string DisplayName, string Status);
}