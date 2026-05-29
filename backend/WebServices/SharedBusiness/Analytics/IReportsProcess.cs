using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.SharedBusiness.Analytics
{
    public interface IReportsProcess
    {
        Task<MonthlyClosingReportDto> GenerateMonthlyClosingReportAsync(int year, int month);
    }
}
