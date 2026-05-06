using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class SeedPrescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DBPatients",
                columns: new[] { "Id", "CreatedAt", "DateOfBirth", "Email", "FirstName", "LastName", "Phone" },
                values: new object[] { 1, new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "juan@test.com", "Juan", "Perez", "12345678" });

            migrationBuilder.InsertData(
                table: "DBPrescriptions",
                columns: new[] { "Id", "DoctorId", "EncounterId", "IssueDate", "PatientId" },
                values: new object[] { 1, 1, null, new DateOnly(2026, 4, 30), 1 });

            migrationBuilder.InsertData(
                table: "PrescriptionMedications",
                columns: new[] { "Id", "Dosage", "MedicationName", "PrescriptionId", "Refills" },
                values: new object[,]
                {
                    { 1, "500mg cada 8 horas", "Paracetamol", 1, 2 },
                    { 2, "400mg cada 12 horas", "Ibuprofeno", 1, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PrescriptionMedications",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PrescriptionMedications",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DBPrescriptions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DBPatients",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
