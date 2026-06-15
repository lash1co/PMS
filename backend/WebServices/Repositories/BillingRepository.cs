using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.SharedBusiness;

namespace WebServices.Repositories
{
    public class BillingRepository
    {
        private readonly DatabaseContext _databaseContext;

        public BillingRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<UpsertRequest> CreateInvoice(InvoiceRequest invoice, Encounter encounter)
        {
            try
            {
                if (invoice.InvoiceDetails is null || !invoice.InvoiceDetails.Any())
                {
                    return new UpsertRequest { UpsertSuccessfull = false, Message = "Invoice must have at least one detail." };
                }

                if (invoice.InvoiceDetails.Any(d => d.Quantity <= 0 || d.UnitPrice < 0))
                {
                    return new UpsertRequest { UpsertSuccessfull = false, Message = "Invoice details must have valid quantity and unit price." };
                }

                var totalAmount = invoice.InvoiceDetails
                    .Sum(d => (d.Quantity * d.UnitPrice));

                await _databaseContext.DBInvoices
                    .AddAsync(new Invoice
                    {
                        Patient = encounter.Patient,
                        Encounter = encounter,
                        Amount = totalAmount,
                        PaidAmount = 0,
                        Status = InvoiceStatus.Pending,
                        IssuedDate = encounter.StartTime,
                        DueDate = DateTime.UtcNow,
                        PaidDate = null,
                        CreatedAt = DateTime.UtcNow,
                        InvoiceDetails = invoice.InvoiceDetails
                            .Select(d => new InvoiceDetail
                            {
                                Code = d.Code,
                                UnitPrice = d.UnitPrice,
                                Quantity = d.Quantity,
                                Price = d.Quantity * d.UnitPrice,
                                Description = d.Description,
                            }).ToList(),
                    });

                await _databaseContext.SaveChangesAsync();

                return new UpsertRequest { UpsertSuccessfull = true, Message = "Invoice created successfully." };
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new UpsertRequest { UpsertSuccessfull = false, Message = errorMsg };
            }
        }

        /// <summary>
        /// Retrieves a filtered list of invoices based on the provided filter criteria.
        /// </summary>
        /// <param name="filter">The filter criteria including patient IDs, date range, and status flags.</param>
        /// <returns>A list of matching <see cref="Invoice"/> entities.</returns>
        public async Task<List<Invoice>> GetFilteredInvoicesAsync(InvoiceHistoryFilterDTO filter)
        {
            var query = _databaseContext.DBInvoices
                .Include(i => i.Patient)
                .Include(i => i.Payments)
                .AsQueryable();
            
             
            if (filter.PatientIds != null && filter.PatientIds.Any())
            {
                query = query.Where(i => filter.PatientIds.Contains(i.PatientId));
            }
            
            if (filter.StartDate.HasValue && filter.EndDate.HasValue)
            {
                var start = filter.StartDate.Value.Date;
                var end = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);

                query = query.Where(i => i.IssuedDate >= start && i.IssuedDate <= end);
            }
             

            if (!filter.IncludeCompleted)
            {
                query = query.Where(i => i.Status != Domain.Entities.InvoiceStatus.Paid);
            }

           
            return await query.OrderByDescending(i => i.IssuedDate).ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific invoice including its entire relational tree.
        /// </summary>
        /// <param name="invoiceId">The unique identifier of the invoice.</param>
        /// <returns>The fully populated <see cref="Invoice"/> entity, or null if not found.</returns>
        public async Task<Invoice?> GetInvoiceWithFullDetailsAsync(int invoiceId)
        {
            return await _databaseContext.DBInvoices
                .Include(i => i.Patient)
                    .ThenInclude(p => p.Insurances) // NO hacer .ThenInclude() de Category porque es un Enum
                .Include(i => i.Encounter)
                    .ThenInclude(e => e.Conditions)
                .Include(i => i.Encounter)
                    .ThenInclude(e => e.Doctor)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

    }
}
