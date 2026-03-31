using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditMediationLocationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_session_nom_mediation_location_mediation_loc~",
                table: "mediation_case_session");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_location_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_session_nom_mediation_location_mediation_loc~",
                table: "mediation_case_session",
                column: "mediation_location_id",
                principalTable: "nom_mediation_location",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_session_nom_mediation_location_mediation_loc~",
                table: "mediation_case_session");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_location_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_session_nom_mediation_location_mediation_loc~",
                table: "mediation_case_session",
                column: "mediation_location_id",
                principalTable: "nom_mediation_location",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
