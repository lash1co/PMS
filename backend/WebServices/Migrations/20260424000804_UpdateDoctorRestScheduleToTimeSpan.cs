using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorRestScheduleToTimeSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "DBDoctorRestSchedules",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EndTime",
                table: "DBDoctorRestSchedules",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "DBDoctorRestSchedules",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndTime", "Reason", "StartTime" },
                values: new object[] { new TimeSpan(0, 15, 0, 0, 0), "Lunch Break", new TimeSpan(0, 14, 0, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "DBDoctorRestSchedules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "DBDoctorRestSchedules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.UpdateData(
                table: "DBDoctorRestSchedules",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndTime", "Reason", "StartTime" },
                values: new object[] { new DateTime(2026, 4, 22, 15, 0, 0, 0, DateTimeKind.Unspecified), "Break", new DateTime(2026, 4, 22, 14, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}
