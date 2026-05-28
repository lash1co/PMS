using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;

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

                encounter.Status = EncounterStatus.Completed;

                await _databaseContext.SaveChangesAsync();

                return new UpsertRequest { UpsertSuccessfull = true, Message = "Invoice created successfully." };
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new UpsertRequest { UpsertSuccessfull = false, Message = errorMsg };
            }
        }
    }
}
