using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    public partial class AddDateForNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ManagerId",
                table: "Notifications",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Employees_ManagerId",
                table: "Notifications",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Employees_ManagerId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ManagerId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Notifications");
        }
    }
}
