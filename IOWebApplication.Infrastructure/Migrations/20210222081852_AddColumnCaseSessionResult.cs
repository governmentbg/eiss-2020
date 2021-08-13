using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnCaseSessionResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_law_unit_select_id",
                table: "case_session_result",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_result_case_law_unit_select_id",
                table: "case_session_result",
                column: "case_law_unit_select_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_result_case_lawunit_case_law_unit_select_id",
                table: "case_session_result",
                column: "case_law_unit_select_id",
                principalTable: "case_lawunit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_result_case_lawunit_case_law_unit_select_id",
                table: "case_session_result");

            migrationBuilder.DropIndex(
                name: "IX_case_session_result_case_law_unit_select_id",
                table: "case_session_result");

            migrationBuilder.DropColumn(
                name: "case_law_unit_select_id",
                table: "case_session_result");
        }
    }
}
