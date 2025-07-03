using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderLineFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                table: "SalesOrderLines",
                newName: "LineNumber");

            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "PurchaseOrderLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "PurchaseOrderLines");

            migrationBuilder.RenameColumn(
                name: "LineNumber",
                table: "SalesOrderLines",
                newName: "PurchaseOrderId");
        }
    }
}
