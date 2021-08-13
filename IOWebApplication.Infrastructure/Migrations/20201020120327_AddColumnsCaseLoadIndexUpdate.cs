using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCaseLoadIndexUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_nom_session_result_case_session_result_id",
                table: "case_load_index");

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_case_session_result_case_session_result_id",
                table: "case_load_index",
                column: "case_session_result_id",
                principalTable: "case_session_result",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_case_session_result_case_session_result_id",
                table: "case_load_index");

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_nom_session_result_case_session_result_id",
                table: "case_load_index",
                column: "case_session_result_id",
                principalTable: "nom_session_result",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
