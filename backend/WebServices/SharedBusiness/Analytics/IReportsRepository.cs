using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.SharedBusiness.Analytics
{
    public interface IReportsRepository
    {
        Task<MonthlyClosingReportDto> GetMonthlyClosingReportAsync(int year, int month);
    }
}
