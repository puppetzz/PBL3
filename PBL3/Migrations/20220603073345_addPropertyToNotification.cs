using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    public partial class addPropertyToNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Notifications",
                newName: "DatePost");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdate",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerIdUpdated",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateUpdate",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ManagerIdUpdated",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "DatePost",
                table: "Notifications",
                newName: "Date");
        }
    }
}
