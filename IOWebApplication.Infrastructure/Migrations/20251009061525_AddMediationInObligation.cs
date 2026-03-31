using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediationInObligation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mediation_case_session_id",
                table: "money_obligation",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на среща");

            migrationBuilder.AddColumn<int>(
                name: "mediation_mediator_id",
                table: "money_obligation",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на медиатор");

            migrationBuilder.AddColumn<int>(
                name: "mediator_fee_id",
                table: "money_obligation",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на ставки за заплащане на медиатори");

            migrationBuilder.CreateIndex(
                name: "IX_money_obligation_mediation_case_session_id",
                table: "money_obligation",
                column: "mediation_case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_money_obligation_mediation_mediator_id",
                table: "money_obligation",
                column: "mediation_mediator_id");

            migrationBuilder.CreateIndex(
                name: "IX_money_obligation_mediator_fee_id",
                table: "money_obligation",
                column: "mediator_fee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_obligation_common_mediation_mediator_mediation_mediat~",
                table: "money_obligation",
                column: "mediation_mediator_id",
                principalTable: "common_mediation_mediator",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_obligation_common_mediator_fee_mediator_fee_id",
                table: "money_obligation",
                column: "mediator_fee_id",
                principalTable: "common_mediator_fee",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_obligation_mediation_case_session_mediation_case_sess~",
                table: "money_obligation",
                column: "mediation_case_session_id",
                principalTable: "mediation_case_session",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_obligation_common_mediation_mediator_mediation_mediat~",
                table: "money_obligation");

            migrationBuilder.DropForeignKey(
                name: "FK_money_obligation_common_mediator_fee_mediator_fee_id",
                table: "money_obligation");

            migrationBuilder.DropForeignKey(
                name: "FK_money_obligation_mediation_case_session_mediation_case_sess~",
                table: "money_obligation");

            migrationBuilder.DropIndex(
                name: "IX_money_obligation_mediation_case_session_id",
                table: "money_obligation");

            migrationBuilder.DropIndex(
                name: "IX_money_obligation_mediation_mediator_id",
                table: "money_obligation");

            migrationBuilder.DropIndex(
                name: "IX_money_obligation_mediator_fee_id",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "mediation_case_session_id",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "mediation_mediator_id",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "mediator_fee_id",
                table: "money_obligation");
        }
    }
}
