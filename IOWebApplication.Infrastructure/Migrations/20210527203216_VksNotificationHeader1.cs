using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VksNotificationHeader1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Month",
                table: "vks_notification_header",
                newName: "month");

            migrationBuilder.AddColumn<string>(
                name: "paper_edition",
                table: "vks_notification_header",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "paper_edition",
                table: "vks_notification_header");

            migrationBuilder.RenameColumn(
                name: "month",
                table: "vks_notification_header",
                newName: "Month");
        }
    }
}
