namespace WebServices.SharedBusiness.Analytics
{
    public class ReportsDTOs
    {
        public record MonthlyClosingReportDto
        {
            public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
            public string ReportPeriod { get; set; } = string.Empty;
            public ClosingSummaryDto Summary { get; set; } = new();
            public List<DoctorProductivityDto> DoctorMetrics { get; set; } = new();
        }
        public record ClosingSummaryDto
        {
            public decimal TotalRevenue { get; set; }
            public int TotalAppointments { get; set; }
            public int CompletedAppointments { get; set; }
            public int CancelledAppointments { get; set; }
            public int NewPatients { get; set; }
        }
        public record DoctorProductivityDto
        {
            public int DoctorId { get; set; }
            public string DoctorName { get; set; } = string.Empty;
            public int AppointmentsAttended { get; set; }
            public int EncountersCompleted { get; set; }
            public decimal RevenueGenerated { get; set; }
        }
        public record MorbidityReportDto
        {
            public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
            public string ReportPeriod { get; set; } = string.Empty;
            public int TotalDiagnoses { get; set; }
            public List<ConditionFrequencyDto> TopConditions { get; set; } = new();
        }
        public record ConditionFrequencyDto
        {
            public string ConditionCode { get; set; } = string.Empty;
            public string ConditionName { get; set; } = string.Empty;
            public int Frequency { get; set; }
            public double Percentage { get; set; }
        }
        public record AccountsReceivableReportDto
        {
            public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
            public decimal TotalPendingAmount { get; set; }
            public decimal TotalOverdueAmount { get; set; }
            public List<DebtorDto> Debtors { get; set; } = new();
        }
        public record DebtorDto
        {
            public int PatientId { get; set; }
            public string PatientName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public int InvoiceId { get; set; }
            public decimal AmountOwed { get; set; }
            public DateTime DueDate { get; set; }
            public bool IsOverdue { get; set; }
        }
        public record AppointmentEfficiencyReportDto
        {
            public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
            public string ReportPeriod { get; set; } = string.Empty;
            public int TotalScheduled { get; set; }
            public int TotalCompleted { get; set; }
            public int TotalCancelled { get; set; }
            public double CompletionRate { get; set; }
        }
    }
}
