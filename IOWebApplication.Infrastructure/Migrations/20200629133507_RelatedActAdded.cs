using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class RelatedActAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "must_select_related_act",
                table: "nom_act_kind",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "related_act_id",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "related_act_id",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_related_act_id",
                table: "case_session_act",
                column: "related_act_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_case_session_act_related_act_id",
                table: "case_session_act",
                column: "related_act_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_case_session_act_related_act_id",
                table: "case_session_act");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_related_act_id",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "must_select_related_act",
                table: "nom_act_kind");

            migrationBuilder.DropColumn(
                name: "related_act_id",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "related_act_id",
                table: "case_session_act");
        }
    }
}
