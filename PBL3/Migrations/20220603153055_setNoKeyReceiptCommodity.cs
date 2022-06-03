using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3.Migrations
{
    public partial class setNoKeyReceiptCommodity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptCommodities_Commodities_CommodityId1",
                table: "ReceiptCommodities");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptCommodities_Receipts_ReceiptId1",
                table: "ReceiptCommodities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceiptCommodities",
                table: "ReceiptCommodities");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptCommodities_CommodityId1",
                table: "ReceiptCommodities");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptCommodities_ReceiptId1",
                table: "ReceiptCommodities");

            migrationBuilder.DropColumn(
                name: "CommodityId1",
                table: "ReceiptCommodities");

            migrationBuilder.DropColumn(
                name: "ReceiptId1",
                table: "ReceiptCommodities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceiptCommodities",
                table: "ReceiptCommodities",
                columns: new[] { "ReceiptId", "CommodityId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReceiptCommodities",
                table: "ReceiptCommodities");

            migrationBuilder.AddColumn<string>(
                name: "CommodityId1",
                table: "ReceiptCommodities",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptId1",
                table: "ReceiptCommodities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReceiptCommodities",
                table: "ReceiptCommodities",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptCommodities_CommodityId1",
                table: "ReceiptCommodities",
                column: "CommodityId1");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptCommodities_ReceiptId1",
                table: "ReceiptCommodities",
                column: "ReceiptId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptCommodities_Commodities_CommodityId1",
                table: "ReceiptCommodities",
                column: "CommodityId1",
                principalTable: "Commodities",
                principalColumn: "CommodityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptCommodities_Receipts_ReceiptId1",
                table: "ReceiptCommodities",
                column: "ReceiptId1",
                principalTable: "Receipts",
                principalColumn: "ReceiptId");
        }
    }
}
