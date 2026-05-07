using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicationName",
                table: "PrescriptionMedications");

            migrationBuilder.AddColumn<int>(
                name: "MedicationId",
                table: "PrescriptionMedications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Medications",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Paracetamol" },
                    { 2, "Ibuprofeno" }
                });

            migrationBuilder.UpdateData(
                table: "PrescriptionMedications",
                keyColumn: "Id",
                keyValue: 1,
                column: "MedicationId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "PrescriptionMedications",
                keyColumn: "Id",
                keyValue: 2,
                column: "MedicationId",
                value: 2);

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedications_MedicationId",
                table: "PrescriptionMedications",
                column: "MedicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrescriptionMedications_Medications_MedicationId",
                table: "PrescriptionMedications",
                column: "MedicationId",
                principalTable: "Medications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrescriptionMedications_Medications_MedicationId",
                table: "PrescriptionMedications");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_PrescriptionMedications_MedicationId",
                table: "PrescriptionMedications");

            migrationBuilder.DropColumn(
                name: "MedicationId",
                table: "PrescriptionMedications");

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
                value: "Paracetamol");

            migrationBuilder.UpdateData(
                table: "PrescriptionMedications",
                keyColumn: "Id",
                keyValue: 2,
                column: "MedicationName",
                value: "Ibuprofeno");
        }
    }
}
