using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedAutoRestockAmountToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_AutoRestockSupplierId",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "AutoRestockAmount",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProductSupplier",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "integer", nullable: false),
                    SuppliersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSupplier", x => new { x.ProductsId, x.SuppliersId });
                    table.ForeignKey(
                        name: "FK_ProductSupplier_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSupplier_Suppliers_SuppliersId",
                        column: x => x.SuppliersId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSupplier_SuppliersId",
                table: "ProductSupplier",
                column: "SuppliersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_AutoRestockSupplierId",
                table: "Products",
                column: "AutoRestockSupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_AutoRestockSupplierId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ProductSupplier");

            migrationBuilder.DropColumn(
                name: "AutoRestockAmount",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_AutoRestockSupplierId",
                table: "Products",
                column: "AutoRestockSupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }
    }
}
