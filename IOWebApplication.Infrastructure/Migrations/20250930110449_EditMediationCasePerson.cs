using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditMediationCasePerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_person_nom_mediation_person_session_state_me~",
                table: "mediation_case_person");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_person_session_state_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_person_nom_mediation_person_session_state_me~",
                table: "mediation_case_person",
                column: "mediation_person_session_state_id",
                principalTable: "nom_mediation_person_session_state",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_person_nom_mediation_person_session_state_me~",
                table: "mediation_case_person");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_person_session_state_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_person_nom_mediation_person_session_state_me~",
                table: "mediation_case_person",
                column: "mediation_person_session_state_id",
                principalTable: "nom_mediation_person_session_state",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
