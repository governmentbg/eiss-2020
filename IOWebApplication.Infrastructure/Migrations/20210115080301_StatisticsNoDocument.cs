using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class StatisticsNoDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "no_case_code_id",
                table: "nom_excel_report_case_type_row");

            migrationBuilder.DropColumn(
                name: "no_document_type_id",
                table: "nom_excel_report_case_type_row");

            migrationBuilder.DropColumn(
                name: "no_case_code_id",
                table: "nom_excel_report_case_type_col");

            migrationBuilder.DropColumn(
                name: "no_document_type_id",
                table: "nom_excel_report_case_type_col");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "no_case_code_id",
                table: "nom_excel_report_case_type_row",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "no_document_type_id",
                table: "nom_excel_report_case_type_row",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "no_case_code_id",
                table: "nom_excel_report_case_type_col",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "no_document_type_id",
                table: "nom_excel_report_case_type_col",
                nullable: true);
        }
    }
}
