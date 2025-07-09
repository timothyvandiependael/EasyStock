using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedSalesOrderLineRelationWithDispatchLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesOrderLineId",
                table: "DispatchLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchLines_SalesOrderLineId",
                table: "DispatchLines",
                column: "SalesOrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchLines_SalesOrderLines_SalesOrderLineId",
                table: "DispatchLines",
                column: "SalesOrderLineId",
                principalTable: "SalesOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchLines_SalesOrderLines_SalesOrderLineId",
                table: "DispatchLines");

            migrationBuilder.DropIndex(
                name: "IX_DispatchLines_SalesOrderLineId",
                table: "DispatchLines");

            migrationBuilder.DropColumn(
                name: "SalesOrderLineId",
                table: "DispatchLines");
        }
    }
}
