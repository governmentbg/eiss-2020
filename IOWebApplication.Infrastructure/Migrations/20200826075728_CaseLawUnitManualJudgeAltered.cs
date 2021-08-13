using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseLawUnitManualJudgeAltered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_manual_judge_case_lawunit_case_lawunit_id",
                table: "case_lawunit_manual_judge");

            migrationBuilder.RenameColumn(
                name: "case_lawunit_id",
                table: "case_lawunit_manual_judge",
                newName: "lawunit_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_lawunit_manual_judge_case_lawunit_id",
                table: "case_lawunit_manual_judge",
                newName: "IX_case_lawunit_manual_judge_lawunit_id");

            migrationBuilder.AddColumn<int>(
                name: "judge_role_id",
                table: "case_lawunit_manual_judge",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_manual_judge_judge_role_id",
                table: "case_lawunit_manual_judge",
                column: "judge_role_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_manual_judge_nom_judge_role_judge_role_id",
                table: "case_lawunit_manual_judge",
                column: "judge_role_id",
                principalTable: "nom_judge_role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_manual_judge_common_law_unit_lawunit_id",
                table: "case_lawunit_manual_judge",
                column: "lawunit_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_manual_judge_nom_judge_role_judge_role_id",
                table: "case_lawunit_manual_judge");

            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_manual_judge_common_law_unit_lawunit_id",
                table: "case_lawunit_manual_judge");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_manual_judge_judge_role_id",
                table: "case_lawunit_manual_judge");

            migrationBuilder.DropColumn(
                name: "judge_role_id",
                table: "case_lawunit_manual_judge");

            migrationBuilder.RenameColumn(
                name: "lawunit_id",
                table: "case_lawunit_manual_judge",
                newName: "case_lawunit_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_lawunit_manual_judge_lawunit_id",
                table: "case_lawunit_manual_judge",
                newName: "IX_case_lawunit_manual_judge_case_lawunit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_manual_judge_case_lawunit_case_lawunit_id",
                table: "case_lawunit_manual_judge",
                column: "case_lawunit_id",
                principalTable: "case_lawunit",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
