using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelReportCaseCodeString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_excel_report_case_code_row_nom_case_code_case_code_id",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.DropIndex(
                name: "IX_nom_excel_report_case_code_row_case_code_id",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.AlterColumn<string>(
                name: "case_code_id",
                table: "nom_excel_report_case_code_row",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "case_code_id",
                table: "nom_excel_report_case_code_row",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_report_case_code_row_case_code_id",
                table: "nom_excel_report_case_code_row",
                column: "case_code_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_excel_report_case_code_row_nom_case_code_case_code_id",
                table: "nom_excel_report_case_code_row",
                column: "case_code_id",
                principalTable: "nom_case_code",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
