using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServices.DataAccess;
using WebServices.SharedBusiness;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace WebServices.Controllers.Invoices
{
    /// <summary>
    /// Represents the data required to create a new invoice.
    /// Includes the PatientId to link the invoice to a specific patient.
    /// </summary>
    public record InvoiceRequestRecord(int PatientId, decimal Amount, DateTime DueDate);

    /// <summary>
    /// API Controller responsible for managing billing and invoices.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Descomenta esto si las facturas requieren que el usuario esté logueado
    public class InvoicesController : ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly InvoiceProcess _invoiceProcess; // Proceso dedicado a facturas
        private readonly PatientProcess _patientProcess;

        public InvoicesController(
            DatabaseContext dbContext,
            InvoiceProcess invoiceProcess,
            PatientProcess patientProcess)
        {
            _dbContext = dbContext;
            _invoiceProcess = invoiceProcess;
            _patientProcess = patientProcess;
        }

        /// <summary>
        /// Retrieves a list of all pending invoices.
        /// Pending invoices are those associated with encounters that have not yet been paid.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PendingInvoices>>> GetPendingInvoices()
        {
            var paidEncounters = await _dbContext.DBInvoices
                .Where(i => i.Status == InvoiceStatus.Paid)
                .Select(i => i.Encounter!.Id)
                .ToArrayAsync();

            var encounters = await _dbContext.Encounters
                .Where(e => !paidEncounters.Contains(e.Id))
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
                    PatientName = encounter.PatientName,
                    EncounterDate = encounter.EncounterDate,
                    InvoiceDetails = new List<PendingInvoiceDetail>
                    {
                        new PendingInvoiceDetail
                        {
                            Code = "MEDATN",
                            Quantity = 1,
                            Description = "Medical Atention"
                        }
                    }
                };

                var invoiceDetails = await _dbContext.DBPrescriptions
                    .Where(p => p.Encounter.Id == encounter.EncounterId)
                    .Select(p => p.Medications)
                    .FirstOrDefaultAsync();

                if (invoiceDetails != null && invoiceDetails.Any())
                {
                    foreach (var medication in invoiceDetails)
                    {
                        invoice.InvoiceDetails.Add(new PendingInvoiceDetail
                        {
                            Code = "MEDPRE",
                            Quantity = medication.Refills,
                            Description = medication.MedicationName
                        });
                    }
                }

                pendingInvoices.Add(invoice);
            }

            return pendingInvoices;
        }

        /// <summary>
        /// Creates a new invoice and associates it with a specific patient.
        /// </summary>
        /// <param name="request">The data required to create the invoice, including PatientId.</param>
        /// <returns>A success message and the new invoice ID.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestRecord request)
        {
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
                // 2. Crear la factura usando el proceso de negocio adecuado
                // Nota: Si la lógica de creación aún vive en PatientProcess, puedes usar _patientProcess.CreateInvoice. 
                // Lo ideal sería mover esa lógica a _invoiceProcess.CreateInvoice en un futuro.
                var newInvoice = _patientProcess.CreateInvoice(
                    patientEntity,
                    request.Amount,
                    request.DueDate
                );

                // 3. Guardar en base de datos
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Invoice created successfully.", invoiceId = newInvoice.Id });
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
            var invoice = await _dbContext.DBInvoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound(new { message = "Invoice not found." });
            }

            return invoice;
        }
    }
}