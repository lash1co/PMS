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
    }
}
