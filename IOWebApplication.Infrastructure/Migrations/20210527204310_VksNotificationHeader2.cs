using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VksNotificationHeader2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "vks_notification_header_id",
                table: "vks_notification_print_list",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "vks_notification_header_id",
                table: "case_session_notification_list",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "vks_notification_header_id",
                table: "vks_notification_print_list");

            migrationBuilder.DropColumn(
                name: "vks_notification_header_id",
                table: "case_session_notification_list");
        }
    }
}
