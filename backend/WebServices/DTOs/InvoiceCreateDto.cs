using System;

namespace WebServices.DTOs
{
    /// <summary>
    /// Data Transfer Object used to receive the necessary information for creating a new invoice.
    /// </summary>
    public class InvoiceCreateDto
    {
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
    }
}