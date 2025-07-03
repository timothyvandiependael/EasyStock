using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddModelBaseToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CrDate",
                table: "SalesOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CrUser",
                table: "SalesOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LcDate",
                table: "SalesOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LcUser",
                table: "SalesOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CrDate",
                table: "SalesOrderLines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CrUser",
                table: "SalesOrderLines",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LcDate",
                table: "SalesOrderLines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LcUser",
                table: "SalesOrderLines",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CrDate",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CrUser",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LcDate",
                table: "PurchaseOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LcUser",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CrDate",
                table: "PurchaseOrderLines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CrUser",
                table: "PurchaseOrderLines",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LcDate",
                table: "PurchaseOrderLines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LcUser",
                table: "PurchaseOrderLines",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CrUser",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "LcDate",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "LcUser",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CrDate",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "CrUser",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "LcDate",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "LcUser",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "CrDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CrUser",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "LcDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "LcUser",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CrDate",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "CrUser",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "LcDate",
                table: "PurchaseOrderLines");

            migrationBuilder.DropColumn(
                name: "LcUser",
                table: "PurchaseOrderLines");
        }
    }
}
