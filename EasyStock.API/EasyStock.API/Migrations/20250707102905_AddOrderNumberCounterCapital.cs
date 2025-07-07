using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberCounterCapital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_orderNumberCounters",
                table: "orderNumberCounters");

            migrationBuilder.RenameTable(
                name: "orderNumberCounters",
                newName: "OrderNumberCounters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderNumberCounters",
                table: "OrderNumberCounters",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderNumberCounters",
                table: "OrderNumberCounters");

            migrationBuilder.RenameTable(
                name: "OrderNumberCounters",
                newName: "orderNumberCounters");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orderNumberCounters",
                table: "orderNumberCounters",
                column: "Id");
        }
    }
}
