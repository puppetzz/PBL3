using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    public partial class addEnterpriseTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Customers_CustomerId",
                table: "Receipts");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Receipts",
                newName: "ContactId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_CustomerId",
                table: "Receipts",
                newName: "IX_Receipts_ContactId");

            migrationBuilder.CreateTable(
                name: "Enterprises",
                columns: table => new
                {
                    EnterpriseId = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    EnterpriseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnterprisePhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnterpriseAdress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enterprises", x => x.EnterpriseId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Customers_ContactId",
                table: "Receipts",
                column: "ContactId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Enterprises_ContactId",
                table: "Receipts",
                column: "ContactId",
                principalTable: "Enterprises",
                principalColumn: "EnterpriseId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Customers_ContactId",
                table: "Receipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Enterprises_ContactId",
                table: "Receipts");

            migrationBuilder.DropTable(
                name: "Enterprises");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "Receipts",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_ContactId",
                table: "Receipts",
                newName: "IX_Receipts_CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Customers_CustomerId",
                table: "Receipts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
