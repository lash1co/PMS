using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DBDoctors",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Aurelio" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreationDate", "Email", "IsActive", "Name", "Password", "Role", "UserName" },
                values: new object[] { -3, new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "doctorPMS@unosquare.com", true, "PMS Doctor", "387D800C0CC82412028CE6435ABC708A52C075D8ED8F9854FBE24691B5E46D8C", "DOCTOR", "doctor" });

            migrationBuilder.InsertData(
                table: "DBDoctorRestSchedules",
                columns: new[] { "Id", "DoctorId", "EndTime", "Reason", "StartTime" },
                values: new object[] { 1, 1, new DateTime(2026, 4, 22, 15, 0, 0, 0, DateTimeKind.Unspecified), "Break", new DateTime(2026, 4, 22, 14, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DBDoctorRestSchedules",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "DBDoctors",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
