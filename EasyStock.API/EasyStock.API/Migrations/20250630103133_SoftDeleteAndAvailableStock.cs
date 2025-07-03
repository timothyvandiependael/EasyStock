using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteAndAvailableStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "Suppliers",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "Suppliers",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "StockMovement",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "StockMovement",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "SalesOrders",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "SalesOrders",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "SalesOrderLines",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "SalesOrderLines",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "PurchaseOrders",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "PurchaseOrders",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "PurchaseOrderLines",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "PurchaseOrderLines",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "PhysicalStock",
                table: "Products",
                newName: "TotalStock");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "Products",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "Products",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "Clients",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "Clients",
                newName: "CrUserId");

            migrationBuilder.RenameColumn(
                name: "LcUser",
                table: "Categories",
                newName: "LcUserId");

            migrationBuilder.RenameColumn(
                name: "CrUser",
                table: "Categories",
                newName: "CrUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "Suppliers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "Suppliers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "StockMovement",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "StockMovement",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "SalesOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "SalesOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "SalesOrderLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "SalesOrderLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "PurchaseOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "PurchaseOrderLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "PurchaseOrderLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailableStock",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "Clients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "Clients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlDate",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlUserId",
                table: "Categories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "AvailableStock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BlDate",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BlUserId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "Suppliers",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "Suppliers",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "StockMovement",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "StockMovement",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "SalesOrders",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "SalesOrders",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "SalesOrderLines",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "SalesOrderLines",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "PurchaseOrders",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "PurchaseOrders",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "PurchaseOrderLines",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "PurchaseOrderLines",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "TotalStock",
                table: "Products",
                newName: "PhysicalStock");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "Products",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "Products",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "Clients",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "Clients",
                newName: "CrUser");

            migrationBuilder.RenameColumn(
                name: "LcUserId",
                table: "Categories",
                newName: "LcUser");

            migrationBuilder.RenameColumn(
                name: "CrUserId",
                table: "Categories",
                newName: "CrUser");
        }
    }
}
