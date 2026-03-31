using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditAppraisals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mediation_case_session_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на среща");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_mediation_case_session_id",
                table: "mediation_case_mediator_appraisal",
                column: "mediation_case_session_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_mediator_appraisal_mediation_case_session_me~",
                table: "mediation_case_mediator_appraisal",
                column: "mediation_case_session_id",
                principalTable: "mediation_case_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_mediator_appraisal_mediation_case_session_me~",
                table: "mediation_case_mediator_appraisal");

            migrationBuilder.DropIndex(
                name: "IX_mediation_case_mediator_appraisal_mediation_case_session_id",
                table: "mediation_case_mediator_appraisal");

            migrationBuilder.DropColumn(
                name: "mediation_case_session_id",
                table: "mediation_case_mediator_appraisal");
        }
    }
}
