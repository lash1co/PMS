using Domain.Entities;
using Domain.Exceptions;
using System;

namespace WebServices.SharedBusiness
{
    public class InvoiceProcess
    {
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
    }
}
