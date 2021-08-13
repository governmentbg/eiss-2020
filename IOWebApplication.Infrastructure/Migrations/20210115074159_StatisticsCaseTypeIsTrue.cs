using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class StatisticsCaseTypeIsTrue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_true",
                table: "nom_excel_report_case_type_row",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_true",
                table: "nom_excel_report_case_type_col",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_true",
                table: "nom_excel_report_case_type_row");

            migrationBuilder.DropColumn(
                name: "is_true",
                table: "nom_excel_report_case_type_col");
        }
    }
}
