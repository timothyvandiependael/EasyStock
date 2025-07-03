using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    QuantityChange = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: true),
                    SalesOrderId = table.Column<int>(type: "integer", nullable: true),
                    CrDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LcDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrUser = table.Column<string>(type: "text", nullable: false),
                    LcUser = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovement_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_ProductId",
                table: "StockMovement",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovement");
        }
    }
}
