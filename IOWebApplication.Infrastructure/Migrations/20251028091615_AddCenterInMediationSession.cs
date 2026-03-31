using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCenterInMediationSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_mediation_coordinator_common_mediation_center_Mediat~",
                table: "common_mediation_coordinator");

            migrationBuilder.DropForeignKey(
                name: "FK_common_mediation_mediator_common_mediation_center_Mediation~",
                table: "common_mediation_mediator");

            migrationBuilder.DropIndex(
                name: "IX_common_mediation_mediator_MediationCenterId",
                table: "common_mediation_mediator");

            migrationBuilder.DropIndex(
                name: "IX_common_mediation_coordinator_MediationCenterId",
                table: "common_mediation_coordinator");

            migrationBuilder.DropColumn(
                name: "MediationCenterId",
                table: "common_mediation_mediator");

            migrationBuilder.DropColumn(
                name: "MediationCenterId",
                table: "common_mediation_coordinator");

            migrationBuilder.AddColumn<int>(
                name: "mediation_centers_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на център");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_mediation_centers_id",
                table: "mediation_case_session",
                column: "mediation_centers_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_session_common_mediation_center_mediation_ce~",
                table: "mediation_case_session",
                column: "mediation_centers_id",
                principalTable: "common_mediation_center",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_session_common_mediation_center_mediation_ce~",
                table: "mediation_case_session");

            migrationBuilder.DropIndex(
                name: "IX_mediation_case_session_mediation_centers_id",
                table: "mediation_case_session");

            migrationBuilder.DropColumn(
                name: "mediation_centers_id",
                table: "mediation_case_session");

            migrationBuilder.AddColumn<int>(
                name: "MediationCenterId",
                table: "common_mediation_mediator",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MediationCenterId",
                table: "common_mediation_coordinator",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_MediationCenterId",
                table: "common_mediation_mediator",
                column: "MediationCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_MediationCenterId",
                table: "common_mediation_coordinator",
                column: "MediationCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_common_mediation_coordinator_common_mediation_center_Mediat~",
                table: "common_mediation_coordinator",
                column: "MediationCenterId",
                principalTable: "common_mediation_center",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_mediation_mediator_common_mediation_center_Mediation~",
                table: "common_mediation_mediator",
                column: "MediationCenterId",
                principalTable: "common_mediation_center",
                principalColumn: "id");
        }
    }
}
