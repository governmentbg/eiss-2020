using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelReportIndex2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_group_id",
                table: "nom_excel_report_index",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_report_index_case_group_id",
                table: "nom_excel_report_index",
                column: "case_group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_excel_report_index_nom_case_group_case_group_id",
                table: "nom_excel_report_index",
                column: "case_group_id",
                principalTable: "nom_case_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_excel_report_index_nom_case_group_case_group_id",
                table: "nom_excel_report_index");

            migrationBuilder.DropIndex(
                name: "IX_nom_excel_report_index_case_group_id",
                table: "nom_excel_report_index");

            migrationBuilder.DropColumn(
                name: "case_group_id",
                table: "nom_excel_report_index");
        }
    }
}
