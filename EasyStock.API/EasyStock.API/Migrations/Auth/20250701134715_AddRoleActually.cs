using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyStock.API.Migrations.Auth
{
    /// <inheritdoc />
    public partial class AddRoleActually : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "UserAuths",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "UserAuths");
        }
    }
}
