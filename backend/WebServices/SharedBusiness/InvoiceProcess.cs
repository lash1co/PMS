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

            if (invoice.PaidAmount + paymentAmount > invoice.Amount)
                throw new DomainException("Payment exceeds invoice total.");

            invoice.PaidAmount += paymentAmount;

            if (invoice.PaidAmount == invoice.Amount)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
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