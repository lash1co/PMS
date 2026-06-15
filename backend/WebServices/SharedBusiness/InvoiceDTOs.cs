namespace WebServices.SharedBusiness
{
    /// <summary>
    /// Data transfer object used to filter the invoice history.
    /// </summary>
    public record InvoiceHistoryFilterDTO
    {
        /// <summary>
        /// Gets the list of patient identifiers to filter by.
        /// </summary>
        public List<int>? PatientIds { get; init; }

        /// <summary>
        /// Gets the start date for the invoice filter.
        /// </summary>
        public DateTime? StartDate { get; init; }

        /// <summary>
        /// Gets the end date for the invoice filter.
        /// </summary>
        public DateTime? EndDate { get; init; }

        /// <summary>
        /// Gets a value indicating whether completed invoices should be included in the results.
        /// </summary>
        public bool IncludeCompleted { get; init; }
    }

    /// <summary>
    /// Data transfer object representing the result of an invoice history query.
    /// </summary>
    public record InvoiceHistoryResultDTO
    {
        /// <summary>
        /// Gets the unique identifier of the invoice.
        /// </summary>
        public int InvoiceId { get; init; }

        /// <summary>
        /// Gets the full name of the patient.
        /// </summary>
        public string PatientName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date the invoice was issued.
        /// </summary>
        public DateTime InvoiceDate { get; init; }

        /// <summary>
        /// Gets the current status of the invoice.
        /// </summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>
        /// Gets the total amount billed in the invoice.
        /// </summary>
        public decimal TotalAmount { get; init; }

        /// <summary>
        /// Gets the total amount paid towards the invoice.
        /// </summary>
        public decimal PaidAmount { get; init; }

        /// <summary>
        /// Gets the remaining balance to be paid.
        /// </summary>
        public decimal Balance { get; init; }

        /// <summary>
        /// Comprehensive record containing all necessary details to display an invoice summary.
        /// </summary>
        public record InvoiceDetailedViewRecord(
            InvoiceHeaderSummary Header,
            PatientInsuranceSummary Patient,
            ClinicalContextSummary? ClinicalContext, // Nullable for "Walk-in" scenarios
            IEnumerable<InvoiceItemDetail> Items,
            IEnumerable<PaymentTransactionSummary> PaymentLedger
        );

        public record InvoiceHeaderSummary(
        int InvoiceId,
        string Status,
        DateTime IssuedDate,
        DateTime DueDate,
        DateTime? PaidDate,
        decimal SubTotal,
        decimal PaidAmount,
        decimal Balance
        );

        public record PatientInsuranceSummary(
        int PatientId,
        string FullName,
        string? InsuranceProvider,
        string? PlanCategory
    );

        public record ClinicalContextSummary(
        int EncounterId,
        DateTime EncounterDate,
        string? DoctorName,
        IEnumerable<string> Diagnoses,
        bool IsWalkInCharge
        );

        public record InvoiceItemDetail(
        int DetailId,
        string Code,
        string Description,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal
        );

        public record PaymentTransactionSummary(
        int PaymentId,
        decimal Amount,
        DateTime PaymentDate,
        string? Method,
        string? Reference,
        string? Notes
        );
    }
}