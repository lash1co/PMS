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
            => await _repository.GetMonthlyClosingReportAsync(year, month);

        public async Task<MorbidityReportDto> GenerateMorbidityReportAsync(int year, int month)
            => await _repository.GetMorbidityReportAsync(year, month);

        public async Task<AccountsReceivableReportDto> GenerateAccountsReceivableReportAsync()
            => await _repository.GetAccountsReceivableReportAsync();

        public async Task<AppointmentEfficiencyReportDto> GenerateAppointmentEfficiencyReportAsync(int year, int month)
            => await _repository.GetAppointmentEfficiencyReportAsync(year, month);


    }
}