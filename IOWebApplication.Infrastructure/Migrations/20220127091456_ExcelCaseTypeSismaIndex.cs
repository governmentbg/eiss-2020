using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelCaseTypeSismaIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sisma_index",
                table: "nom_excel_report_case_type_col",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sisma_index",
                table: "nom_excel_report_case_type_col");
        }
    }
}
