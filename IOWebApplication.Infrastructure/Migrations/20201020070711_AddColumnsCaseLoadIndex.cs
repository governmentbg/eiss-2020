using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCaseLoadIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "act_type_id",
                table: "case_load_index",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_session_result_id",
                table: "case_load_index",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "session_result_id",
                table: "case_load_index",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "session_type_id",
                table: "case_load_index",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_load_index_act_type_id",
                table: "case_load_index",
                column: "act_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_load_index_case_session_act_id",
                table: "case_load_index",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_load_index_case_session_id",
                table: "case_load_index",
                column: "case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_load_index_case_session_result_id",
                table: "case_load_index",
                column: "case_session_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_load_index_session_result_id",
                table: "case_load_index",
                column: "session_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_load_index_session_type_id",
                table: "case_load_index",
                column: "session_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_nom_act_type_act_type_id",
                table: "case_load_index",
                column: "act_type_id",
                principalTable: "nom_act_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_case_session_act_case_session_act_id",
                table: "case_load_index",
                column: "case_session_act_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_case_session_case_session_id",
                table: "case_load_index",
                column: "case_session_id",
                principalTable: "case_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_nom_session_result_case_session_result_id",
                table: "case_load_index",
                column: "case_session_result_id",
                principalTable: "nom_session_result",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_nom_session_result_session_result_id",
                table: "case_load_index",
                column: "session_result_id",
                principalTable: "nom_session_result",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_load_index_nom_session_type_session_type_id",
                table: "case_load_index",
                column: "session_type_id",
                principalTable: "nom_session_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_nom_act_type_act_type_id",
                table: "case_load_index");

            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_case_session_act_case_session_act_id",
                table: "case_load_index");

            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_case_session_case_session_id",
                table: "case_load_index");

            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_nom_session_result_case_session_result_id",
                table: "case_load_index");

            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_nom_session_result_session_result_id",
                table: "case_load_index");

            migrationBuilder.DropForeignKey(
                name: "FK_case_load_index_nom_session_type_session_type_id",
                table: "case_load_index");

            migrationBuilder.DropIndex(
                name: "IX_case_load_index_act_type_id",
                table: "case_load_index");

            migrationBuilder.DropIndex(
                name: "IX_case_load_index_case_session_act_id",
                table: "case_load_index");

            migrationBuilder.DropIndex(
                name: "IX_case_load_index_case_session_id",
                table: "case_load_index");

            migrationBuilder.DropIndex(
                name: "IX_case_load_index_case_session_result_id",
                table: "case_load_index");

            migrationBuilder.DropIndex(
                name: "IX_case_load_index_session_result_id",
                table: "case_load_index");

            migrationBuilder.DropIndex(
                name: "IX_case_load_index_session_type_id",
                table: "case_load_index");

            migrationBuilder.DropColumn(
                name: "act_type_id",
                table: "case_load_index");

            migrationBuilder.DropColumn(
                name: "case_session_result_id",
                table: "case_load_index");

            migrationBuilder.DropColumn(
                name: "session_result_id",
                table: "case_load_index");

            migrationBuilder.DropColumn(
                name: "session_type_id",
                table: "case_load_index");
        }
    }
}
