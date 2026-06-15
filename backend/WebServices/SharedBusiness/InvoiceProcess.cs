using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebServices.Repositories;
using static WebServices.SharedBusiness.InvoiceHistoryResultDTO;

namespace WebServices.SharedBusiness
{
    public class InvoiceProcess
    {
        private readonly BillingRepository _billingRepository;

        public InvoiceProcess(BillingRepository billingRepository)
        {
            _billingRepository = billingRepository;
        }

        public void RegisterPayment(Invoice invoice, decimal paymentAmount)
        {
            if (paymentAmount <= 0)
                throw new DomainException("Payment amount must be greater than zero.");

            if (invoice.Status == InvoiceStatus.Paid)
                throw new DomainException("Invoice is already fully paid.");

            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new DomainException("Cancelled invoice cannot receive payments.");

            if (invoice.PaidAmount + paymentAmount > invoice.Amount)
                throw new DomainException("Payment exceeds invoice total.");

            invoice.PaidAmount += paymentAmount;

            if (invoice.PaidAmount == invoice.Amount)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
            }
            else
            {
                invoice.Status = InvoiceStatus.Pending;
                invoice.PaidDate = null;
            }
        }

        public decimal GetBalance(Invoice invoice)
        {
            return invoice.Amount - invoice.PaidAmount;
        }

        public void EnsurePatientCanCreateInvoice(Patient patient)
        {
            if (patient.Invoices?.Any(i =>
                i.Status == InvoiceStatus.Pending ||
                i.Status == InvoiceStatus.Overdue) == true)
            {
                throw new DomainException("Patient already has an active invoice. It must be fully paid before creating a new one.");
            }
        }

        public void MarkAsOverdue(Invoice invoice)
        {
            if (invoice.Status == InvoiceStatus.Paid)
                throw new DomainException("Paid invoice cannot be marked as overdue.");

            invoice.Status = InvoiceStatus.Overdue;
        }

        public void Cancel(Invoice invoice)
        {
            if (invoice.PaidAmount > 0)
                throw new DomainException("Cannot cancel an invoice with payments.");

            invoice.Status = InvoiceStatus.Cancelled;
        }

        /// <summary>
        /// Processes the invoice history retrieval and maps the domain entities to result DTOs.
        /// </summary>
        /// <param name="filter">The filter criteria for the invoice history.</param>
        /// <returns>A collection of mapped <see cref="InvoiceHistoryResultDTO"/>.</returns>
        public async Task<List<InvoiceHistoryResultDTO>> GetInvoiceHistoryAsync(InvoiceHistoryFilterDTO filter)
        {
            var invoices = await _billingRepository.GetFilteredInvoicesAsync(filter);

            return invoices.Select(i =>
            {
                var totalAmount = i.Amount;
                var paidAmount = i.Payments?.Sum(p => p.Amount) ?? 0m;

                return new InvoiceHistoryResultDTO
                {
                    InvoiceId = i.Id,
                    PatientName = $"{i.Patient.FirstName} {i.Patient.LastName}".Trim(),
                    InvoiceDate = i.IssuedDate,
                    Status = i.Status.ToString(), 
                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    Balance = totalAmount - paidAmount
                };
            }).ToList();
        }

        /// <summary>
        /// Retrieves the detailed summary of an invoice, mapping the domain tree to immutable records.
        /// Throws a DomainException if the invoice does not exist.
        /// </summary>
        public async Task<InvoiceDetailedViewRecord> GetInvoiceDetailedSummaryAsync(int invoiceId)
        {
            var invoice = await _billingRepository.GetInvoiceWithFullDetailsAsync(invoiceId);

            if (invoice == null)
            {
                throw new DomainException("The specified invoice was not found.");
            }

            var subTotal = invoice.Amount;
            var paidAmount = invoice.Payments?.Sum(p => p.Amount) ?? 0m;
            var balance = subTotal - paidAmount;

            var activeInsurance = invoice.Patient.Insurances?.FirstOrDefault();

            var insuranceSummary = new PatientInsuranceSummary(
                invoice.PatientId,
                $"{invoice.Patient.FirstName} {invoice.Patient.LastName}".Trim(),
                activeInsurance?.PayerName,
                activeInsurance?.Category.ToString()
            );

            ClinicalContextSummary? clinicalContext = null;
            if (invoice.Encounter != null)
            {
                var diagnoses = invoice.Encounter.Conditions?
                    .Select(c => !string.IsNullOrWhiteSpace(c.DisplayName) ? c.DisplayName : (c.Code ?? "Unknown Diagnosis"))
                    .ToList() ?? new List<string>();

                var doctorName = invoice.Encounter.Doctor != null
                    ? invoice.Encounter.Doctor.Name
                    : "Unknown Doctor";

                clinicalContext = new ClinicalContextSummary(
                    invoice.Encounter.Id,
                    invoice.Encounter.StartTime,
                    doctorName,
                    diagnoses,
                    IsWalkInCharge: false
                );
            }

            var items = invoice.InvoiceDetails?
                .Select(d => new InvoiceItemDetail(
                    d.Id,
                    d.Code,
                    d.Description,
                    d.Quantity,
                    d.UnitPrice,
                    d.Price
                )).ToList() ?? new List<InvoiceItemDetail>();

            var ledger = invoice.Payments?
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentTransactionSummary(
                    p.Id,
                    p.Amount,
                    p.PaymentDate,
                    p.PaymentMethod,
                    p.ReferenceNumber,
                    p.Notes
                )).ToList() ?? new List<PaymentTransactionSummary>();

            return new InvoiceDetailedViewRecord(
                new InvoiceHeaderSummary(
                    invoice.Id,
                    invoice.Status.ToString(),
                    invoice.IssuedDate,
                    invoice.DueDate,
                    invoice.PaidDate,
                    subTotal,
                    paidAmount,
                    balance
                ),
                insuranceSummary,
                clinicalContext,
                items,
                ledger
            );
        }
    }
}