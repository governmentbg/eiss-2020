using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EpepCasePersonIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_mediator_appraisal_id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                comment: " Идентификатор на оценка",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на медиатор в дело");

            migrationBuilder.AddColumn<int>(
                name: "epep_case_person_id",
                table: "case_notification_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "epep_case_person_id",
                table: "case_notification",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_epep_case_person_id",
                table: "case_notification",
                column: "epep_case_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_notification_case_person_epep_case_person_id",
                table: "case_notification",
                column: "epep_case_person_id",
                principalTable: "case_person",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_notification_case_person_epep_case_person_id",
                table: "case_notification");

            migrationBuilder.DropIndex(
                name: "IX_case_notification_epep_case_person_id",
                table: "case_notification");

            migrationBuilder.DropColumn(
                name: "epep_case_person_id",
                table: "case_notification_h");

            migrationBuilder.DropColumn(
                name: "epep_case_person_id",
                table: "case_notification");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_mediator_appraisal_id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на медиатор в дело",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: " Идентификатор на оценка");
        }
    }
}
