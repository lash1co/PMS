using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServices.Migrations
{
    /// <inheritdoc />
    public partial class DoctorUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "DBDoctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "DBDoctors",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserId",
                value: -3);

            migrationBuilder.CreateIndex(
                name: "IX_DBDoctors_UserId",
                table: "DBDoctors",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DBDoctors_Users_UserId",
                table: "DBDoctors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DBDoctors_Users_UserId",
                table: "DBDoctors");

            migrationBuilder.DropIndex(
                name: "IX_DBDoctors_UserId",
                table: "DBDoctors");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DBDoctors");
        }
    }
}
