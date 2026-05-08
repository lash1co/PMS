using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class CleanDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicationName",
                table: "PrescriptionMedications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "PrescriptionMedications",
                keyColumn: "Id",
                keyValue: 1,
                column: "MedicationName",
                value: null);

            migrationBuilder.UpdateData(
                table: "PrescriptionMedications",
                keyColumn: "Id",
                keyValue: 2,
                column: "MedicationName",
                value: null);
        }
    }
}
