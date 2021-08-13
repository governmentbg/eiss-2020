using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentResolutionSecondJudge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "judge_decision_count",
                table: "document_resolution",
                nullable: true,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<int>(
                name: "judge_decision_lawunit2_id",
                table: "document_resolution",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "judge_decision_user2_id",
                table: "document_resolution",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_judge_decision_lawunit2_id",
                table: "document_resolution",
                column: "judge_decision_lawunit2_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_judge_decision_user2_id",
                table: "document_resolution",
                column: "judge_decision_user2_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_resolution_common_law_unit_judge_decision_lawunit2~",
                table: "document_resolution",
                column: "judge_decision_lawunit2_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_document_resolution_identity_users_judge_decision_user2_id",
                table: "document_resolution",
                column: "judge_decision_user2_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_resolution_common_law_unit_judge_decision_lawunit2~",
                table: "document_resolution");

            migrationBuilder.DropForeignKey(
                name: "FK_document_resolution_identity_users_judge_decision_user2_id",
                table: "document_resolution");

            migrationBuilder.DropIndex(
                name: "IX_document_resolution_judge_decision_lawunit2_id",
                table: "document_resolution");

            migrationBuilder.DropIndex(
                name: "IX_document_resolution_judge_decision_user2_id",
                table: "document_resolution");

            migrationBuilder.DropColumn(
                name: "judge_decision_count",
                table: "document_resolution");

            migrationBuilder.DropColumn(
                name: "judge_decision_lawunit2_id",
                table: "document_resolution");

            migrationBuilder.DropColumn(
                name: "judge_decision_user2_id",
                table: "document_resolution");
        }
    }
}
