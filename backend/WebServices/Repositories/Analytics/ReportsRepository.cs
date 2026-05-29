using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebServices.DataAccess;
using WebServices.SharedBusiness.Analytics;
using static WebServices.SharedBusiness.Analytics.ReportsDTOs;

namespace WebServices.Repositories.Analytics
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly DatabaseContext _context;

        public ReportsRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<MonthlyClosingReportDto> GetMonthlyClosingReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1).ToUniversalTime();
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            var appointments = await _context.DBAppointments
                .Where(a => a.StartTime >= startDate && a.StartTime <= endDate)
                .ToListAsync();

            var newPatientsCount = await _context.DBPatients
                .CountAsync(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

            var totalRevenue = await _context.DBPayments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .SumAsync(p => p.Amount);

            var doctors = await _context.DBDoctors.ToListAsync();
            var doctorMetrics = new List<DoctorProductivityDto>();

            var encounters = await _context.Encounters
                .Where(e => e.StartTime >= startDate && e.StartTime <= endDate)
                .ToListAsync();

            var invoicesWithEncounters = await _context.DBInvoices
                .Include(i => i.Encounter)
                .Include(i => i.Payments)
                .Where(i => i.IssuedDate >= startDate && i.IssuedDate <= endDate && i.Encounter != null)
                .ToListAsync();

            foreach (var doc in doctors)
            {
                var docAppointments = appointments.Where(a => a.DoctorId == doc.Id).ToList();
                var docEncounters = encounters.Where(e => e.DoctorId == doc.Id).ToList();

                var docRevenue = invoicesWithEncounters
                    .Where(i => i.Encounter!.DoctorId == doc.Id)
                    .SelectMany(i => i.Payments!)
                    .Sum(p => p.Amount);

                doctorMetrics.Add(new DoctorProductivityDto
                {
                    DoctorId = doc.Id,
                    DoctorName = $"Dr. {doc.Name}",
                    AppointmentsAttended = docAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                    EncountersCompleted = docEncounters.Count(e => e.Status == EncounterStatus.Completed),
                    RevenueGenerated = docRevenue
                });
            }

            return new MonthlyClosingReportDto
            {
                ReportPeriod = $"{startDate:MMMM yyyy}",
                Summary = new ClosingSummaryDto
                {
                    TotalRevenue = totalRevenue,
                    TotalAppointments = appointments.Count,
                    CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                    NewPatients = newPatientsCount
                },
                DoctorMetrics = doctorMetrics.OrderByDescending(d => d.RevenueGenerated).ToList()
            };


        }

        public async Task<MorbidityReportDto> GetMorbidityReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1).ToUniversalTime();
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            var conditions = await _context.Conditions
                .Where(c => c.RecordedDate >= startDate && c.RecordedDate <= endDate)
                .ToListAsync();

            var totalDiagnoses = conditions.Count;

            var topConditions = conditions
                .GroupBy(c => new { c.Code, c.DisplayName })
                .Select(g => new ConditionFrequencyDto
                {
                    ConditionCode = g.Key.Code,
                    ConditionName = g.Key.DisplayName,
                    Frequency = g.Count(),
                    Percentage = totalDiagnoses > 0 ? Math.Round((double)g.Count() / totalDiagnoses * 100, 2) : 0
                })
                .OrderByDescending(c => c.Frequency)
                .Take(10) // Top 10 enfermedades
                .ToList();

            return new MorbidityReportDto
            {
                ReportPeriod = $"{startDate:MMMM yyyy}",
                TotalDiagnoses = totalDiagnoses,
                TopConditions = topConditions
            };
        }

        public async Task<AccountsReceivableReportDto> GetAccountsReceivableReportAsync()
        {
            var now = DateTime.UtcNow;
            // Status 1 = Pending, Status 3 = Overdue
            var pendingInvoices = await _context.DBInvoices
                .Include(i => i.Patient)
                .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
                .ToListAsync();

            var debtors = pendingInvoices.Select(i => new DebtorDto
            {
                PatientId = i.PatientId,
                PatientName = $"{i.Patient!.FirstName} {i.Patient.LastName}",
                Phone = i.Patient.Phone,
                InvoiceId = i.Id,
                AmountOwed = i.Amount - i.PaidAmount,
                DueDate = i.DueDate,
                IsOverdue = i.DueDate < now
            })
            .OrderByDescending(d => d.AmountOwed)
            .ToList();

            return new AccountsReceivableReportDto
            {
                TotalPendingAmount = debtors.Where(d => !d.IsOverdue).Sum(d => d.AmountOwed),
                TotalOverdueAmount = debtors.Where(d => d.IsOverdue).Sum(d => d.AmountOwed),
                Debtors = debtors
            };
        }

        public async Task<AppointmentEfficiencyReportDto> GetAppointmentEfficiencyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1).ToUniversalTime();
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            var appointments = await _context.DBAppointments
                .Where(a => a.StartTime >= startDate && a.StartTime <= endDate)
                .ToListAsync();

            var total = appointments.Count;
            var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
            var cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);

            return new AppointmentEfficiencyReportDto
            {
                ReportPeriod = $"{startDate:MMMM yyyy}",
                TotalScheduled = total,
                TotalCompleted = completed,
                TotalCancelled = cancelled,
                CompletionRate = total > 0 ? Math.Round((double)completed / total * 100, 2) : 0
            };
        }
    }
}