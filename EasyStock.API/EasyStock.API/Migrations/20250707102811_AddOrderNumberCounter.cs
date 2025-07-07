using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SalesOrders",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SalesOrderLines",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseOrderLineId",
                table: "ReceptionLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PurchaseOrders",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PurchaseOrderLines",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "orderNumberCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderType = table.Column<string>(type: "varchar(2)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    LastNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderNumberCounters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionLines_PurchaseOrderLineId",
                table: "ReceptionLines",
                column: "PurchaseOrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceptionLines_PurchaseOrderLines_PurchaseOrderLineId",
                table: "ReceptionLines",
                column: "PurchaseOrderLineId",
                principalTable: "PurchaseOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceptionLines_PurchaseOrderLines_PurchaseOrderLineId",
                table: "ReceptionLines");

            migrationBuilder.DropTable(
                name: "orderNumberCounters");

            migrationBuilder.DropIndex(
                name: "IX_ReceptionLines_PurchaseOrderLineId",
                table: "ReceptionLines");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderLineId",
                table: "ReceptionLines");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SalesOrders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SalesOrderLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PurchaseOrders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PurchaseOrderLines",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");
        }
    }
}
