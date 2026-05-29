using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.SharedBusiness.Analytics
{
    public interface IReportsProcess
    {
        Task<MonthlyClosingReportDto> GenerateMonthlyClosingReportAsync(int year, int month);
        Task<MorbidityReportDto> GenerateMorbidityReportAsync(int year, int month);

        Task<AccountsReceivableReportDto> GenerateAccountsReceivableReportAsync();

        Task<AppointmentEfficiencyReportDto> GenerateAppointmentEfficiencyReportAsync(int year, int month);
    }
}
