using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentNotification2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "document_person_address_id",
                table: "document_notification",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notification_link_name",
                table: "document_notification",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notification_person_name",
                table: "document_notification",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notification_person_role",
                table: "document_notification",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_person_address_id",
                table: "document_notification");

            migrationBuilder.DropColumn(
                name: "notification_link_name",
                table: "document_notification");

            migrationBuilder.DropColumn(
                name: "notification_person_name",
                table: "document_notification");

            migrationBuilder.DropColumn(
                name: "notification_person_role",
                table: "document_notification");
        }
    }
}
