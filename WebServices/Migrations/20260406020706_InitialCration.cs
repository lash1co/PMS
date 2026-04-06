using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class InitialCration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DBDoctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBDoctors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DBPatients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBPatients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DBAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DBAppointments_DBDoctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DBDoctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DBAppointments_DBPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "DBPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DBMedicalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Vitals = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagnosisCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBMedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DBMedicalRecords_DBPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "DBPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DBPrescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Medication = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Refils = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DBPrescriptions_DBDoctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DBDoctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DBPrescriptions_DBPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "DBPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DBAppointments_DoctorId",
                table: "DBAppointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DBAppointments_PatientId",
                table: "DBAppointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DBMedicalRecords_PatientId",
                table: "DBMedicalRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DBPrescriptions_DoctorId",
                table: "DBPrescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DBPrescriptions_PatientId",
                table: "DBPrescriptions",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DBAppointments");

            migrationBuilder.DropTable(
                name: "DBMedicalRecords");

            migrationBuilder.DropTable(
                name: "DBPrescriptions");

            migrationBuilder.DropTable(
                name: "DBDoctors");

            migrationBuilder.DropTable(
                name: "DBPatients");
        }
    }
}
