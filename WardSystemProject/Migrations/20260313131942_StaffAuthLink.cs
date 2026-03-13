using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WardSystemProject.Migrations
{
    /// <inheritdoc />
    public partial class StaffAuthLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Staff",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WardId",
                table: "Staff",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_WardId",
                table: "Staff",
                column: "WardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Wards_WardId",
                table: "Staff",
                column: "WardId",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Wards_WardId",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_WardId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "Staff");
        }
    }
}
