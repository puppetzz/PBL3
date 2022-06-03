using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    public partial class updateNotificationModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Employees_ManagerId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Notifications",
                newName: "ManagerIdPost");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_ManagerId",
                table: "Notifications",
                newName: "IX_Notifications_ManagerIdPost");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Employees_ManagerIdPost",
                table: "Notifications",
                column: "ManagerIdPost",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Employees_ManagerIdPost",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "ManagerIdPost",
                table: "Notifications",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_ManagerIdPost",
                table: "Notifications",
                newName: "IX_Notifications_ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Employees_ManagerId",
                table: "Notifications",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
