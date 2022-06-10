using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    public partial class addResetPasswordVerifiedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VerifiedAt",
                table: "Accounts",
                newName: "VerifiedResetPasswordAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAccountAt",
                table: "Accounts",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerifiedAccountAt",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "VerifiedResetPasswordAt",
                table: "Accounts",
                newName: "VerifiedAt");
        }
    }
}
