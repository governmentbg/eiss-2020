using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ElectronicDocumentCreateUserAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "create_user_name",
                table: "electronic_document",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "from_api",
                table: "electronic_document",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "create_user_name",
                table: "electronic_document");

            migrationBuilder.DropColumn(
                name: "from_api",
                table: "electronic_document");
        }
    }
}
