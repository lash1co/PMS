using System;
using Domain.Entities;
using Domain.Exceptions;
using WebServices.SharedBusiness;
using Xunit;

namespace Tests.Invoices
{
    public class InvoiceTests
    {
        private readonly InvoiceProcess _invoiceProcess = new InvoiceProcess();

        [Fact]
        public void Should_Create_invoice_with_valid_data()
        {
            var invoice = new Invoice(patientId: 1, amount: 1500m, dueDate: DateTime.Today.AddDays(30));
            Assert.NotNull(invoice);
            Assert.Equal(1500m, invoice.Amount);
            Assert.Equal(0m, invoice.PaidAmount);
            Assert.Equal(InvoiceStatus.Pending, invoice.Status);
        }

        [Fact]
        public void Should_register_partial_payment()
        {
            var invoice = new Invoice(1, 1000m, DateTime.Today.AddDays(10));

            _invoiceProcess.RegisterPayment(invoice, 400m);

            Assert.Equal(400m, invoice.PaidAmount);
            Assert.Equal(InvoiceStatus.Pending, invoice.Status);
        }

        [Fact]
        public void Should_mark_invoice_as_paid_when_fully_paid()
        {
            var invoice = new Invoice(1, 1000m, DateTime.Today.AddDays(10));

            _invoiceProcess.RegisterPayment(invoice, 1000m);

            Assert.Equal(InvoiceStatus.Paid, invoice.Status);
            Assert.NotNull(invoice.PaidDate);
        }

        [Fact]
        public void Should_not_allow_payment_exceeding_invoice_amount()
        {
            var invoice = new Invoice(1, 1000m, DateTime.Today.AddDays(10));
            Assert.Throws<DomainException>(() => _invoiceProcess.RegisterPayment(invoice, 1200m));
        }

        [Fact]
        public void Should_not_allow_paying_paid_invoice()
        {
            var invoice = new Invoice(1, 1000m, DateTime.Today.AddDays(10));
            _invoiceProcess.RegisterPayment(invoice, 1000m);
            Assert.Throws<DomainException>(() => _invoiceProcess.RegisterPayment(invoice, 100m));
        }

        [Fact]
        public void Should_mark_invoice_as_overdue()
        {
            var invoice = new Invoice(1, 500m, DateTime.Today.AddDays(10));
            _invoiceProcess.MarkAsOverdue(invoice);
            Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
        }

        [Fact]
        public void Should_not_cancel_invoice_with_payments()
        {
            var invoice = new Invoice(1, 800m, DateTime.Today.AddDays(10));
            _invoiceProcess.RegisterPayment(invoice, 200m);
            Assert.Throws<DomainException>(() => _invoiceProcess.Cancel(invoice));
        }
        [Fact]
        public void Should_cancel_invoice_without_payments()
        {
            // Arrange
            var invoice = new Invoice(1, 800m, DateTime.Today.AddDays(10));
            Assert.Equal(0m, invoice.PaidAmount);

            // Act
            _invoiceProcess.Cancel(invoice);

            // Assert
            Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        }
    }
}