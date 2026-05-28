using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebServices.DataAccess;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260528000100_InvoicePayments")]
    public partial class InvoicePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DBInvoices_PatientId",
                table: "DBInvoices");

            migrationBuilder.CreateTable(
                name: "DBPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DBPayments_DBInvoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "DBInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DBInvoices_PatientId",
                table: "DBInvoices",
                column: "PatientId",
                unique: true,
                filter: "[Status] IN (1, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_DBPayments_InvoiceId",
                table: "DBPayments",
                column: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DBPayments");

            migrationBuilder.DropIndex(
                name: "IX_DBInvoices_PatientId",
                table: "DBInvoices");

            migrationBuilder.CreateIndex(
                name: "IX_DBInvoices_PatientId",
                table: "DBInvoices",
                column: "PatientId");
        }
    }
}
