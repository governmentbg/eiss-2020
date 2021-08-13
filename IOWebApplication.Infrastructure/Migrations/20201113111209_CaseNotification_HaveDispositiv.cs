using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseNotification_HaveDispositiv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "have_dispositiv",
                table: "case_notification_h",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_from_email",
                table: "case_notification_h",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "have_dispositiv",
                table: "case_notification",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_from_email",
                table: "case_notification",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "have_dispositiv",
                table: "case_notification_h");

            migrationBuilder.DropColumn(
                name: "is_from_email",
                table: "case_notification_h");

            migrationBuilder.DropColumn(
                name: "have_dispositiv",
                table: "case_notification");

            migrationBuilder.DropColumn(
                name: "is_from_email",
                table: "case_notification");
        }
    }
}
