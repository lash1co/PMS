using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.SharedBusiness.Analytics
{
    public interface IReportsRepository
    {
        Task<MonthlyClosingReportDto> GetMonthlyClosingReportAsync(int year, int month);
        Task<MorbidityReportDto> GetMorbidityReportAsync(int year, int month);
        Task<AccountsReceivableReportDto> GetAccountsReceivableReportAsync();
        Task<AppointmentEfficiencyReportDto> GetAppointmentEfficiencyReportAsync(int year, int month);
    }
}
