using Domain.Entities;
using Domain.Exceptions;
using Domain.SharedConstants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.Repositories;
using WebServices.SharedBusiness;

namespace WebServices.Controllers.Invoices
{
    /// <summary>
    /// Represents the data required to create a new invoice.
    /// Includes the PatientId to link the invoice to a specific patient.
    /// </summary>
    public record InvoiceRequestRecord(int PatientId, decimal Amount, DateTime DueDate);
    public record PaymentRequestRecord(decimal Amount, string? PaymentMethod, string? ReferenceNumber, string? Notes);
    public record InvoicePaymentRecord(int Id, decimal Amount, DateTime PaymentDate, string? PaymentMethod, string? ReferenceNumber, string? Notes);
    public record InvoiceDetailRecord(int Id, string Code, decimal UnitPrice, int Quantity, decimal Price, string Description);
    public record InvoiceResponseRecord(
        int Id,
        string PatientName,
        decimal Amount,
        decimal PaidAmount,
        decimal Balance,
        InvoiceStatus Status,
        DateTime IssuedDate,
        DateTime DueDate,
        DateTime? PaidDate,
        IEnumerable<InvoiceDetailRecord> InvoiceDetails,
        IEnumerable<InvoicePaymentRecord> Payments);

    /// <summary>
    /// API Controller responsible for managing billing and invoices.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DatabaseContext _dbContext;
        private readonly InvoiceProcess _invoiceProcess; // Proceso dedicado a facturas
        private readonly PatientProcess _patientProcess;
        private readonly List<string> _authorizedRoles = new List<string>
        {
            UserConstants.RoleConstants.AdminRole,
            UserConstants.RoleConstants.BillingRole
        };

        public InvoicesController(
            IConfiguration config,
            DatabaseContext dbContext,
            InvoiceProcess invoiceProcess,
            PatientProcess patientProcess)
        {
            _config = config;
            _dbContext = dbContext;
            _invoiceProcess = invoiceProcess;
            _patientProcess = patientProcess;
        }

        /// <summary>
        /// Retrieves a list of all pending invoices.
        /// Pending invoices are those associated with encounters that have not yet been paid.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PendingInvoices>>> GetPendingInvoices()
        {
            var invoicedEncounters = await _dbContext.DBInvoices
                .Where(i => i.Encounter != null)
                .Select(i => i.Encounter!.Id)
                .ToArrayAsync();

            var patientsWithActiveInvoices = await _dbContext.DBInvoices
                .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
                .Select(i => i.PatientId)
                .ToArrayAsync();

            var encounters = await _dbContext.Encounters
                .Where(e => !invoicedEncounters.Contains(e.Id) &&
                    !patientsWithActiveInvoices.Contains(e.PatientId) &&
                    e.Status == EncounterStatus.Completed)
                .Select(e => new
                {
                    EncounterId = e.Id,
                    PatientName = e.Patient.LastName + " " + e.Patient.FirstName,
                    EncounterDate = DateOnly.FromDateTime(e.StartTime)
                })
                .ToListAsync();

            var pendingInvoices = new List<PendingInvoices>();

            foreach (var encounter in encounters)
            {
                var invoice = new PendingInvoices
                {
                    EncounterId = encounter.EncounterId,
                    PatientName = encounter.PatientName,
                    EncounterDate = encounter.EncounterDate,
                    InvoiceDetails = new List<PendingInvoiceDetail>
                    {
                        new PendingInvoiceDetail
                        {
                            Code = "MEDATN",
                            Quantity = 1,
                            UnitPrice = 0,
                            Price = 0,
                            Description = "Medical Atention"
                        }
                    }
                };

                var invoiceMedicationDetails = await _dbContext.PrescriptionMedications
                    .Where(m => m.Prescription.Encounter.Id == encounter.EncounterId)
                    .Select(m => new PendingInvoiceDetail{
                        Code = "MEDPRSC",
                        Quantity = m.Refills,
                        UnitPrice = 0,
                        Price = 0,
                        Description = m.Medication!.Name
                    })
                    .ToListAsync();

                invoice.InvoiceDetails.AddRange(invoiceMedicationDetails);

                var encounterLaboratories = await _dbContext.EncounterLaboratories
                    .Include(l => l.LaboratoriesDetails)
                        .ThenInclude(d => d.Laboratory)
                    .Where(l => l.Encounter != null && l.Encounter.Id == encounter.EncounterId)
                    .ToListAsync();

                var invoiceLaboratoryDetails = encounterLaboratories
                    .SelectMany(l => l.LaboratoriesDetails ?? new List<EncounterLaboratoriesDetail>())
                    .Where(d => d.Laboratory != null)
                    .Select(d =>
                    {
                        var price = d.Laboratory!.Price.HasValue ? Convert.ToDecimal(d.Laboratory.Price.Value) : 0;
                        return new PendingInvoiceDetail
                        {
                            Code = "LAB",
                            Quantity = 1,
                            UnitPrice = price,
                            Price = price,
                            Description = d.Laboratory.Description
                        };
                    })
                    .ToList();

                invoice.InvoiceDetails.AddRange(invoiceLaboratoryDetails);

                pendingInvoices.Add(invoice);
            }

            return pendingInvoices;
        }

        /// <summary>
        /// Creates a new invoice and associates it with a specific patient.
        /// </summary>
        /// <param name="request">The data required to create the invoice, including PatientId.</param>
        /// <returns>A success message and the new invoice ID.</returns>
        [HttpPost("createInvoice")]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestRecord request)
        {
            var validationProcess = new TokenValidationProcess(_config, _dbContext);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            // 1. Verificar que el paciente exista
            var patientEntity = await _dbContext.DBPatients
                .Include(p => p.Invoices)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId);

            if (patientEntity == null)
            {
                return NotFound(new { message = "Patient not found." });
            }

            try
            {
                _invoiceProcess.EnsurePatientCanCreateInvoice(patientEntity);

                // 2. Crear la factura usando el proceso de negocio adecuado
                // Nota: Si la lógica de creación aún vive en PatientProcess, puedes usar _patientProcess.CreateInvoice. 
                // Lo ideal sería mover esa lógica a _invoiceProcess.CreateInvoice en un futuro.
                var newInvoice = _patientProcess.CreateInvoice(
                    patientEntity,
                    request.Amount,
                    request.DueDate
                );
                newInvoice.Patient = patientEntity;
                _dbContext.DBInvoices.Add(newInvoice);

                // 3. Guardar en base de datos
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Invoice created successfully.", invoiceId = newInvoice.Id });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //Accessible via api/invoices/createBilling
        [HttpPost("createBilling")]
        public async Task<ActionResult<UpsertRequest>> CreateBilling([FromBody] InvoiceRequest billingRecord)
        {
            var validationProcess = new TokenValidationProcess(_config, _dbContext);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var encounter = await _dbContext.Encounters
                .Include(x => x.Patient)
                    .ThenInclude(p => p.Invoices)
                .Where(e => e.Id == billingRecord.EncounterId)
                .FirstOrDefaultAsync();

            if (encounter is null)
            {
                return NotFound("The specified Encounter does not exist.");
            }

            if (encounter.Status != EncounterStatus.Completed)
            {
                return BadRequest(new { error = "Only completed encounters can be billed." });
            }

            try
            {
                _invoiceProcess.EnsurePatientCanCreateInvoice(encounter.Patient);
            }
            catch (DomainException ex)
            {
                return Conflict(new { error = ex.Message });
            }

            var billingRepository = new BillingRepository(_dbContext);
            var billingProcessResult = await billingRepository.CreateInvoice(billingRecord, encounter);

            if (!billingProcessResult.UpsertSuccessfull)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    billingProcessResult.Message
                );
            }

            return Ok(billingProcessResult);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<InvoiceResponseRecord>>> GetActiveInvoices()
        {
            var validationProcess = new TokenValidationProcess(_config, _dbContext);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var invoices = await _dbContext.DBInvoices
                .Include(i => i.Patient)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Payments)
                .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();

            return Ok(invoices.Select(ToInvoiceResponse));
        }

        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<InvoicePaymentRecord>>> GetInvoicePayments(int id)
        {
            var validationProcess = new TokenValidationProcess(_config, _dbContext);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var invoiceExists = await _dbContext.DBInvoices.AnyAsync(i => i.Id == id);
            if (!invoiceExists)
            {
                return NotFound(new { message = "Invoice not found." });
            }

            return Ok(await _dbContext.DBPayments
                .Where(p => p.InvoiceId == id)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new InvoicePaymentRecord(
                    p.Id,
                    p.Amount,
                    p.PaymentDate,
                    p.PaymentMethod,
                    p.ReferenceNumber,
                    p.Notes))
                .ToListAsync());
        }

        [HttpPost("{id}/payments")]
        public async Task<ActionResult<InvoiceResponseRecord>> RegisterPayment(int id, [FromBody] PaymentRequestRecord request)
        {
            var validationProcess = new TokenValidationProcess(_config, _dbContext);
            var authResult = await validationProcess.ValidateAuthorizationAsync(Request.Headers["Authorization"], _authorizedRoles);
            if (!authResult.Value.tokenIsValid)
            {
                return StatusCode(authResult.Value.errorStatus, authResult.Value.errorMessage);
            }

            var invoice = await _dbContext.DBInvoices
                .Include(i => i.Patient)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice is null)
            {
                return NotFound(new { message = "Invoice not found." });
            }

            try
            {
                if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                {
                    return BadRequest(new { error = "Payment method is required." });
                }

                _invoiceProcess.RegisterPayment(invoice, request.Amount);

                invoice.Payments ??= new List<Payment>();
                invoice.Payments.Add(new Payment
                {
                    Invoice = invoice,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    ReferenceNumber = request.ReferenceNumber,
                    Notes = request.Notes,
                    PaymentDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync();

                return Ok(ToInvoiceResponse(invoice));
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific invoice by its ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var invoice = await _dbContext.DBInvoices
                .Include(i => i.Patient)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound(new { message = "Invoice not found." });
            }

            return invoice;
        }

        private static InvoiceResponseRecord ToInvoiceResponse(Invoice invoice)
        {
            return new InvoiceResponseRecord(
                invoice.Id,
                invoice.Patient.LastName + " " + invoice.Patient.FirstName,
                invoice.Amount,
                invoice.PaidAmount,
                invoice.Amount - invoice.PaidAmount,
                invoice.Status,
                invoice.IssuedDate,
                invoice.DueDate,
                invoice.PaidDate,
                invoice.InvoiceDetails?
                    .Select(d => new InvoiceDetailRecord(d.Id, d.Code, d.UnitPrice, d.Quantity, d.Price, d.Description))
                    .ToList() ?? new List<InvoiceDetailRecord>(),
                invoice.Payments?
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new InvoicePaymentRecord(p.Id, p.Amount, p.PaymentDate, p.PaymentMethod, p.ReferenceNumber, p.Notes))
                    .ToList() ?? new List<InvoicePaymentRecord>());
        }
    }
}
