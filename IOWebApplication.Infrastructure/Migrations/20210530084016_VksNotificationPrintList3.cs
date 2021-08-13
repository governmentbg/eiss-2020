using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VksNotificationPrintList3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "case_person_id",
                table: "vks_notification_print_list");

            migrationBuilder.AlterColumn<int>(
                name: "case_session_id",
                table: "vks_notification_print_list",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vks_notification_print_list_case_id",
                table: "vks_notification_print_list",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_notification_print_list_case_session_id",
                table: "vks_notification_print_list",
                column: "case_session_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vks_notification_print_list_case_case_id",
                table: "vks_notification_print_list",
                column: "case_id",
                principalTable: "case",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vks_notification_print_list_case_session_case_session_id",
                table: "vks_notification_print_list",
                column: "case_session_id",
                principalTable: "case_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vks_notification_print_list_case_case_id",
                table: "vks_notification_print_list");

            migrationBuilder.DropForeignKey(
                name: "FK_vks_notification_print_list_case_session_case_session_id",
                table: "vks_notification_print_list");

            migrationBuilder.DropIndex(
                name: "IX_vks_notification_print_list_case_id",
                table: "vks_notification_print_list");

            migrationBuilder.DropIndex(
                name: "IX_vks_notification_print_list_case_session_id",
                table: "vks_notification_print_list");

            migrationBuilder.AlterColumn<int>(
                name: "case_session_id",
                table: "vks_notification_print_list",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "case_person_id",
                table: "vks_notification_print_list",
                nullable: true);
        }
    }
}
