using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelReportIndex3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActComplainIndex",
                table: "nom_excel_report_index",
                newName: "act_complain_index");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "act_complain_index",
                table: "nom_excel_report_index",
                newName: "ActComplainIndex");
        }
    }
}
