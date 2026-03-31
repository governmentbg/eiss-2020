using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankFileDirectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "file_directory",
                table: "nom_bank",
                type: "text",
                nullable: true,
                comment: "Име на директория, в която се пращат файловете");

            migrationBuilder.AddColumn<int>(
                name: "file_structure_type",
                table: "nom_bank",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Тип на структура на файла");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_directory",
                table: "nom_bank");

            migrationBuilder.DropColumn(
                name: "file_structure_type",
                table: "nom_bank");
        }
    }
}
