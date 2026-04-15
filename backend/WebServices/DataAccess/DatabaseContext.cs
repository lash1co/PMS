using Domain.Entities;
using Domain.SharedConstants;
using Microsoft.EntityFrameworkCore;

namespace WebServices.DataAccess
{
    /// <summary>
    /// Data base context definiton
    /// </summary>
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Patient> DBPatients { get; set; }

        public DbSet<Doctor> DBDoctors { get; set; }

        public DbSet<Invoice> DBInvoices { get; set; }

        public DbSet<MedicalRecord> DBMedicalRecords { get; set; }

        public DbSet<Appointment> DBAppointments { get; set; }

        public DbSet<Prescriptions> DBPrescriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        Id = -1,
                        Name = "PMS Administrator",
                        Email = "adminPMS@unosquare.com",
                        IsActive = true,
                        UserName = "admin",
                        Password = "387D800C0CC82412028CE6435ABC708A52C075D8ED8F9854FBE24691B5E46D8C", // Control123 (HEX codification)
                        CreationDate = new DateTime(2026, 4, 9),
                        Role = UserConstants.RoleConostants.AdminRole,
                    }
                );

            modelBuilder.Entity<Patient>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Doctor>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.PaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Invoices);

            modelBuilder.Entity<Appointment>()
                .HasKey(ap => ap.Id);

            modelBuilder.Entity<Appointment>()
                .Property(ap => ap.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Appointment>()
                .HasOne(ap => ap.Doctor)
                .WithMany(d => d.Appointments);

            modelBuilder.Entity<Appointment>()
                .HasOne(p => p.Patient)
                .WithMany(ap => ap.Appointments);

            modelBuilder.Entity<MedicalRecord>()
                .HasKey(mr => mr.Id);

            modelBuilder.Entity<MedicalRecord>()
                .Property(mr => mr.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Patient)
                .WithMany(p => p.MedicalRecords);

            modelBuilder.Entity<Prescriptions>()
                .HasKey(pr => pr.Id);

            modelBuilder.Entity<Prescriptions> ()
                .Property(pr => pr.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Prescriptions>()
                .HasOne(pr => pr.Doctor)
                .WithMany(d => d.Prescriptions);

            modelBuilder.Entity<Prescriptions>()
                .HasOne(pr => pr.Patient)
                .WithMany(p => p.Prescriptions);
        }
    }
}
