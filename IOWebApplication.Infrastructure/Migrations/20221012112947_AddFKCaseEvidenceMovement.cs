using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddFKCaseEvidenceMovement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_case_evidence_movement_case_session_act_id",
                table: "case_evidence_movement",
                column: "case_session_act_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_evidence_movement_case_session_act_case_session_act_id",
                table: "case_evidence_movement",
                column: "case_session_act_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_evidence_movement_case_session_act_case_session_act_id",
                table: "case_evidence_movement");

            migrationBuilder.DropIndex(
                name: "IX_case_evidence_movement_case_session_act_id",
                table: "case_evidence_movement");
        }
    }
}
