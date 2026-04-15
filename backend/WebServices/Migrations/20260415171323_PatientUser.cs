using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class PatientUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreationDate", "Email", "IsActive", "Name", "Password", "Role", "UserName" },
                values: new object[] { -2, new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "patientPMS@unosquare.com", true, "PMS Patient", "387D800C0CC82412028CE6435ABC708A52C075D8ED8F9854FBE24691B5E46D8C", "PATIENT", "patient" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -2);
        }
    }
}
