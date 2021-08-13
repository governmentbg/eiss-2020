using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelReportCaseCodeRow1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_excel_report_case_code_row",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    excel_report_template_id = table.Column<int>(nullable: false),
                    case_code_id = table.Column<int>(nullable: false),
                    sheet_index = table.Column<int>(nullable: false),
                    row_index = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_excel_report_case_code_row", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_excel_report_case_code_row_nom_case_code_case_code_id",
                        column: x => x.case_code_id,
                        principalTable: "nom_case_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_excel_report_case_code_row_common_excel_report_template~",
                        column: x => x.excel_report_template_id,
                        principalTable: "common_excel_report_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_report_case_code_row_case_code_id",
                table: "nom_excel_report_case_code_row",
                column: "case_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_report_case_code_row_excel_report_template_id",
                table: "nom_excel_report_case_code_row",
                column: "excel_report_template_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_excel_report_case_code_row");
        }
    }
}
