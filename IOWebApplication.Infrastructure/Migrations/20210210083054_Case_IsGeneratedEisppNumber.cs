using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class Case_IsGeneratedEisppNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_generated_eispp_number",
                table: "case_h",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_generated_eispp_number",
                table: "case_crimes",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_generated_eispp_number",
                table: "case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_generated_eispp_number",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "is_generated_eispp_number",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "is_generated_eispp_number",
                table: "case");
        }
    }
}
