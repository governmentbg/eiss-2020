using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelReportComplainResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_excel_report_case_code_row_common_excel_report_template~",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.DropIndex(
                name: "IX_nom_excel_report_case_code_row_excel_report_template_id",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.DropColumn(
                name: "excel_report_template_id",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.AddColumn<int>(
                name: "court_type_id",
                table: "nom_excel_report_case_code_row",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_excel_report_complain_result",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_type_id = table.Column<int>(nullable: false),
                    sheet_index = table.Column<int>(nullable: false),
                    act_complain_result = table.Column<string>(nullable: true),
                    col_index = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_excel_report_complain_result", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_excel_report_complain_result_nom_court_type_court_type_~",
                        column: x => x.court_type_id,
                        principalTable: "nom_court_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_report_case_code_row_court_type_id",
                table: "nom_excel_report_case_code_row",
                column: "court_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_report_complain_result_court_type_id",
                table: "nom_excel_report_complain_result",
                column: "court_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_excel_report_case_code_row_nom_court_type_court_type_id",
                table: "nom_excel_report_case_code_row",
                column: "court_type_id",
                principalTable: "nom_court_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_excel_report_case_code_row_nom_court_type_court_type_id",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.DropTable(
                name: "nom_excel_report_complain_result");

            migrationBuilder.DropIndex(
                name: "IX_nom_excel_report_case_code_row_court_type_id",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.DropColumn(
                name: "court_type_id",
                table: "nom_excel_report_case_code_row");

            migrationBuilder.AddColumn<int>(
                name: "excel_report_template_id",
                table: "nom_excel_report_case_code_row",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_report_case_code_row_excel_report_template_id",
                table: "nom_excel_report_case_code_row",
                column: "excel_report_template_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_excel_report_case_code_row_common_excel_report_template~",
                table: "nom_excel_report_case_code_row",
                column: "excel_report_template_id",
                principalTable: "common_excel_report_template",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
