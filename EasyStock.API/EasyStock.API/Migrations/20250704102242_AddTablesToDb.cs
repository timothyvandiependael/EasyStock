using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DispatchNumber = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    CrDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LcDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrUserId = table.Column<string>(type: "text", nullable: false),
                    LcUserId = table.Column<string>(type: "text", nullable: false),
                    BlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispatches_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceptionNumber = table.Column<string>(type: "text", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    CrDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LcDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrUserId = table.Column<string>(type: "text", nullable: false),
                    LcUserId = table.Column<string>(type: "text", nullable: false),
                    BlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receptions_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: false),
                    CanView = table.Column<bool>(type: "boolean", nullable: false),
                    CanAdd = table.Column<bool>(type: "boolean", nullable: false),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    CrDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LcDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrUserId = table.Column<string>(type: "text", nullable: false),
                    LcUserId = table.Column<string>(type: "text", nullable: false),
                    BlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DispatchLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DispatchId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CrDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LcDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrUserId = table.Column<string>(type: "text", nullable: false),
                    LcUserId = table.Column<string>(type: "text", nullable: false),
                    BlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchLines_Dispatches_DispatchId",
                        column: x => x.DispatchId,
                        principalTable: "Dispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DispatchLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceptionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceptionId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CrDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LcDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrUserId = table.Column<string>(type: "text", nullable: false),
                    LcUserId = table.Column<string>(type: "text", nullable: false),
                    BlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceptionLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceptionLines_Receptions_ReceptionId",
                        column: x => x.ReceptionId,
                        principalTable: "Receptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_ClientId",
                table: "Dispatches",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchLines_DispatchId",
                table: "DispatchLines",
                column: "DispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchLines_ProductId",
                table: "DispatchLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionLines_ProductId",
                table: "ReceptionLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionLines_ReceptionId",
                table: "ReceptionLines",
                column: "ReceptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Receptions_SupplierId",
                table: "Receptions",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchLines");

            migrationBuilder.DropTable(
                name: "ReceptionLines");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "Dispatches");

            migrationBuilder.DropTable(
                name: "Receptions");
        }
    }
}
