using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsComplexIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "complex_index_actual",
                table: "case_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "complex_index_legal",
                table: "case_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "complex_index_actual",
                table: "case",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "complex_index_legal",
                table: "case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "complex_index_actual",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "complex_index_legal",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "complex_index_actual",
                table: "case");

            migrationBuilder.DropColumn(
                name: "complex_index_legal",
                table: "case");
        }
    }
}
