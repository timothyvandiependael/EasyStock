using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyStock.API.Migrations
{
    /// <inheritdoc />
    public partial class UserAndAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    CrDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LcDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CrUserId = table.Column<string>(type: "text", nullable: false),
                    LcUserId = table.Column<string>(type: "text", nullable: false),
                    BlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
