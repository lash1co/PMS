using System.Threading.Tasks;
using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.SharedBusiness.Analytics
{   
    public class ReportsProcess : IReportsProcess
    {
        private readonly IReportsRepository _repository;

        public ReportsProcess(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<MonthlyClosingReportDto> GenerateMonthlyClosingReportAsync(int year, int month)
        {
            // Here we could add additional business logic before fetching the report data, such as:
            // validation of role permissions, audit trail of who requested the report 
            return await _repository.GetMonthlyClosingReportAsync(year, month);
        }
    }
}