using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.EntityFrameworkCore;

namespace WebServices.DataAccess
{
    /// <summary>
    /// Represents the main database context for the Patient Management System (PMS).
    /// Manages entity configurations, relationships, and database sets.
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
        {
        }

        // =========================================================
        // SYSTEM ENTITIES
        // =========================================================

        /// <summary>System users including Administrators, Patients, and Doctors.</summary>
        public DbSet<User> Users { get; set; }

        /// <summary>Registered patients in the system.</summary>
        public DbSet<Patient> DBPatients { get; set; }

        /// <summary>Registered healthcare providers (Doctors).</summary>
        public DbSet<Doctor> DBDoctors { get; set; }

        // =========================================================
        // BILLING & INSURANCE ENTITIES
        // =========================================================

        /// <summary>Financial invoices billed to patients.</summary>
        public DbSet<Invoice> DBInvoices { get; set; }

        /// <summary>Medical insurance policies linked to patients.</summary>
        public DbSet<Insurance> DBInsurances { get; set; }

        // =========================================================
        // SCHEDULING ENTITIES
        // =========================================================

        /// <summary>Scheduled medical appointments.</summary>
        public DbSet<Appointment> DBAppointments { get; set; }

        /// <summary>Recurring daily break times for doctors.</summary>
        public DbSet<DoctorRestSchedule> DBDoctorRestSchedules { get; set; }

        /// <summary>Read-only SQL View combining appointments for the calendar UI.</summary>
        public DbSet<ScheduleView> DBScheduleViews { get; set; }

        // =========================================================
        // CLINICAL WORKFLOW ENTITIES (FHIR Inspired)
        // =========================================================

        /// <summary>The main clinical encounter/visit record connecting a patient and doctor.</summary>
        public DbSet<Encounter> Encounters { get; set; }

        /// <summary>Vital signs and objective measurements taken during an encounter.</summary>
        public DbSet<ClinicalObservation> ClinicalObservations { get; set; }

        /// <summary>SOAP narrative notes written by the doctor during an encounter.</summary>
        public DbSet<ClinicalNote> ClinicalNotes { get; set; }

        /// <summary>Medical diagnoses or problems identified.</summary>
        public DbSet<Condition> Conditions { get; set; }

        /// <summary>Medical interventions or services performed.</summary>
        public DbSet<Procedure> Procedures { get; set; }

        /// <summary>Allergies or adverse reactions registered for a patient.</summary>
        public DbSet<AllergyIntolerance> AllergyIntolerances { get; set; }

        /// <summary>Medication prescriptions issued by a doctor to a patient.</summary>
        public DbSet<Prescriptions> DBPrescriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // SYSTEM & USERS CONFIGURATION
            // =========================================================
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = -1,
                    Name = "PMS Administrator",
                    Email = "adminPMS@unosquare.com",
                    IsActive = true,
                    UserName = "admin",
                    Password = "387D800C0CC82412028CE6435ABC708A52C075D8ED8F9854FBE24691B5E46D8C", // Control123 (HEX codification)
                    CreationDate = new DateTime(2026, 4, 9),
                    Role = UserConstants.RoleConstants.AdminRole,
                },
                new User
                {
                    Id = -2,
                    Name = "PMS Patient",
                    Email = "patientPMS@unosquare.com",
                    IsActive = true,
                    UserName = "patient",
                    Password = "387D800C0CC82412028CE6435ABC708A52C075D8ED8F9854FBE24691B5E46D8C", // Control123 (HEX codification)
                    CreationDate = new DateTime(2026, 4, 9),
                    Role = UserConstants.RoleConstants.PatientRole,
                },
                new User
                {
                    Id = -3,
                    Name = "PMS Doctor",
                    Email = "doctorPMS@unosquare.com",
                    IsActive = true,
                    UserName = "doctor",
                    Password = "387D800C0CC82412028CE6435ABC708A52C075D8ED8F9854FBE24691B5E46D8C", // Control123 (HEX codification)
                    CreationDate = new DateTime(2026, 4, 9),
                    Role = UserConstants.RoleConstants.DoctorRole,
                }
            );

            modelBuilder.Entity<Patient>().HasKey(p => p.Id);
            modelBuilder.Entity<Patient>().Property(p => p.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Doctor>().HasKey(p => p.Id);
            modelBuilder.Entity<Doctor>().Property(d => d.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor { Id = 1, Name = "Aurelio" }
            );

            // =========================================================
            // INVOICES & INSURANCE CONFIGURATION
            // =========================================================
            modelBuilder.Entity<Invoice>().HasKey(i => i.Id);
            modelBuilder.Entity<Invoice>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Invoice>().Property(i => i.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>().Property(i => i.PaidAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Invoices);

            modelBuilder.Entity<Insurance>().HasKey(i => i.Id);
            modelBuilder.Entity<Insurance>().Property(i => i.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Insurance>().Property(i => i.OfficeVisitCopay).HasPrecision(18, 2);
            modelBuilder.Entity<Insurance>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Insurances)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================================================
            // SCHEDULING CONFIGURATION
            // =========================================================
            modelBuilder.Entity<Appointment>().HasKey(ap => ap.Id);
            modelBuilder.Entity<Appointment>().Property(ap => ap.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Appointment>()
                .HasOne(ap => ap.Doctor)
                .WithMany(d => d.Appointments);
            modelBuilder.Entity<Appointment>()
                .HasOne(p => p.Patient)
                .WithMany(ap => ap.Appointments);

            /* * LEGACY IMPLEMENTATION: DoctorRestSchedule using DateTime.
             * This was replaced by a TimeSpan approach to support daily recurring rests 
             * without tying them to a specific calendar day.
             * modelBuilder.Entity<DoctorRestSchedule>()
                .HasData(
                new
                {
                    Id = 1,
                    DoctorId = 1,
                    StartTime = new DateTime(2026, 4, 22, 14, 0, 0), 
                    EndTime = new DateTime(2026, 4, 22, 15, 0, 0),
                    Reason = "Break"
                }
            );
            */

            modelBuilder.Entity<DoctorRestSchedule>().HasKey(dr => dr.Id);
            modelBuilder.Entity<DoctorRestSchedule>().Property(dr => dr.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<DoctorRestSchedule>()
                .HasOne(dr => dr.Doctor)
                .WithMany(d => d.DoctorRestSchedules);

            modelBuilder.Entity<DoctorRestSchedule>().HasData(
                new
                {
                    Id = 1,
                    DoctorId = 1,
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(15, 0, 0),
                    Reason = "Lunch Break"
                }
            );

            /* * LEGACY SQL VIEW: ScheduleView combining Appointments and Rests via UNION.
             * Replaced because handling recurring TimeSpan rests dynamically in C# business 
             * logic (ScheduleProcess) is more scalable than complex SQL UNIONS.
             *
            modelBuilder.Entity<ScheduleView>(sv =>
            {
                sv.HasNoKey();
                sv.ToSqlQuery<ScheduleView>("SELECT 'Appointment' AS Type," +
                    "a.Id," +
                    "CAST(a.StartTime AS Date) AS Date," +
                    "a.StartTime," +
                    "a.EndTime," +
                    "a.Status AS ScheduleStatus," +
                    "a.Reason AS ScheduleDescription," +
                    "a.DoctorId," +
                    "d.Name AS DoctorName," +
                    "a.PatientId," +
                    "p.FirstName + ' ' + p.LastName AS PatientName " +
                    "FROM DBAppointments a " +
                    "JOIN DBDoctors d ON a.DoctorId = d.Id " +
                    "JOIN DBPatients p ON a.PatientId = p.Id " +
                    "UNION " +
                    "SELECT 'Rest' AS Type," +
                    "dr.Id," +
                    "CAST(dr.StartTime AS Date) AS Date," +
                    "dr.StartTime," +
                    "dr.EndTime," +
                    "'' AS ScheduleStatus," +
                    "dr.Reason AS ScheduleDescription," +
                    "dr.DoctorId," +
                    "d.Name AS DoctorName," +
                    "NULL AS PatientId," +
                    "NULL AS PatientName " +
                    "FROM DBDoctorRestSchedules dr " +
                    "JOIN DBDoctors d ON dr.DoctorId = d.Id");
            });
            */

            modelBuilder.Entity<ScheduleView>(sv =>
            {
                sv.HasNoKey();
                sv.ToSqlQuery<ScheduleView>("SELECT 'Appointment' AS Type," +
                    "a.Id," +
                    "CAST(a.StartTime AS Date) AS Date," +
                    "a.StartTime," +
                    "a.EndTime," +
                    "a.Status AS ScheduleStatus," +
                    "a.Reason AS ScheduleDescription," +
                    "a.DoctorId," +
                    "d.Name AS DoctorName," +
                    "a.PatientId," +
                    "p.FirstName + ' ' + p.LastName AS PatientName " +
                    "FROM DBAppointments a " +
                    "JOIN DBDoctors d ON a.DoctorId = d.Id " +
                    "JOIN DBPatients p ON a.PatientId = p.Id");
            });

            // =========================================================
            // CLINICAL WORKFLOW CONFIGURATION (Encounter, Observations, etc.)
            // =========================================================

            // 1. Enum String Conversions (For database readability)
            modelBuilder.Entity<Encounter>().Property(e => e.Status).HasConversion<string>();
            modelBuilder.Entity<Procedure>().Property(p => p.Status).HasConversion<string>();
            modelBuilder.Entity<Condition>().Property(c => c.ClinicalStatus).HasConversion<string>();
            modelBuilder.Entity<AllergyIntolerance>().Property(a => a.Criticality).HasConversion<string>();

            // 2. Decimal Precision (Prevents data loss warnings)
            modelBuilder.Entity<ClinicalObservation>()
                .Property(c => c.ValueQuantity)
                .HasPrecision(18, 2);

            // 3. Encounter Relationships and Audit Protection (Delete Behaviors)
            modelBuilder.Entity<Encounter>()
                .HasOne(e => e.Patient)
                .WithMany(p => p.Encounters)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Encounter>()
                .HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Encounter>()
                .HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // 4. Prescriptions unified configurations
            modelBuilder.Entity<Prescriptions>().HasKey(pr => pr.Id);
            modelBuilder.Entity<Prescriptions>().Property(pr => pr.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Prescriptions>()
                .HasOne(pr => pr.Doctor)
                .WithMany(d => d.Prescriptions);

            modelBuilder.Entity<Prescriptions>()
                .HasOne(pr => pr.Patient)
                .WithMany(p => p.Prescriptions);

            modelBuilder.Entity<Prescriptions>()
                .HasOne(p => p.Encounter)
                .WithMany(e => e.Prescriptions)
                .HasForeignKey(p => p.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}