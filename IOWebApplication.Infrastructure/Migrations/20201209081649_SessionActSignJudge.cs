using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class SessionActSignJudge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sign_judge_lawunit_id",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sign_judge_lawunit_id",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_sign_judge_lawunit_id",
                table: "case_session_act",
                column: "sign_judge_lawunit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_common_law_unit_sign_judge_lawunit_id",
                table: "case_session_act",
                column: "sign_judge_lawunit_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_common_law_unit_sign_judge_lawunit_id",
                table: "case_session_act");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_sign_judge_lawunit_id",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "sign_judge_lawunit_id",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "sign_judge_lawunit_id",
                table: "case_session_act");
        }
    }
}
