using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class HtmlTemplate_DefaultZoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "default_zoom",
                table: "common_html_template",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vks_notification_print_list_vks_notification_header_id",
                table: "vks_notification_print_list",
                column: "vks_notification_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_notification_list_vks_notification_header_id",
                table: "case_session_notification_list",
                column: "vks_notification_header_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_notification_list_vks_notification_header_vks_~",
                table: "case_session_notification_list",
                column: "vks_notification_header_id",
                principalTable: "vks_notification_header",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vks_notification_print_list_vks_notification_header_vks_not~",
                table: "vks_notification_print_list",
                column: "vks_notification_header_id",
                principalTable: "vks_notification_header",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_notification_list_vks_notification_header_vks_~",
                table: "case_session_notification_list");

            migrationBuilder.DropForeignKey(
                name: "FK_vks_notification_print_list_vks_notification_header_vks_not~",
                table: "vks_notification_print_list");

            migrationBuilder.DropIndex(
                name: "IX_vks_notification_print_list_vks_notification_header_id",
                table: "vks_notification_print_list");

            migrationBuilder.DropIndex(
                name: "IX_case_session_notification_list_vks_notification_header_id",
                table: "case_session_notification_list");

            migrationBuilder.DropColumn(
                name: "default_zoom",
                table: "common_html_template");
        }
    }
}
