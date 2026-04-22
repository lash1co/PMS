using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DBInsurances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    PayerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MemberId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeVisitCopay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RxBIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RxPCN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RxGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelationshipToSubscriber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriberName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriberDOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastVerifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBInsurances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DBInsurances_DBPatients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "DBPatients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DBInsurances_PatientId",
                table: "DBInsurances",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DBInsurances");
        }
    }
}
