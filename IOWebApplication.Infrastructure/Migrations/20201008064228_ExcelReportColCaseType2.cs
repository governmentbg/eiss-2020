using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelReportColCaseType2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "excel_report_col",
                table: "nom_court_type_case_type",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "excel_report_col",
                table: "nom_court_type_case_type",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
