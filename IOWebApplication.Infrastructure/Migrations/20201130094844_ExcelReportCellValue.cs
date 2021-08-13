using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelReportCellValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cell_value_int",
                table: "common_excel_report_data",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cell_value_type",
                table: "common_excel_report_data",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cell_value_int",
                table: "common_excel_report_data");

            migrationBuilder.DropColumn(
                name: "cell_value_type",
                table: "common_excel_report_data");
        }
    }
}
