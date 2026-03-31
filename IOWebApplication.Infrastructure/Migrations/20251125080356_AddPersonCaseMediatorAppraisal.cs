using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonCaseMediatorAppraisal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mediation_case_person_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на страна от делото в среща");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_mediation_case_person_id",
                table: "mediation_case_mediator_appraisal",
                column: "mediation_case_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_mediator_appraisal_mediation_case_person_med~",
                table: "mediation_case_mediator_appraisal",
                column: "mediation_case_person_id",
                principalTable: "mediation_case_person",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_mediator_appraisal_mediation_case_person_med~",
                table: "mediation_case_mediator_appraisal");

            migrationBuilder.DropIndex(
                name: "IX_mediation_case_mediator_appraisal_mediation_case_person_id",
                table: "mediation_case_mediator_appraisal");

            migrationBuilder.DropColumn(
                name: "mediation_case_person_id",
                table: "mediation_case_mediator_appraisal");
        }
    }
}
