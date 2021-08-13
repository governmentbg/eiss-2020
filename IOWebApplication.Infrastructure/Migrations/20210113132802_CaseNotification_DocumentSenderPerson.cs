using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseNotification_DocumentSenderPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "have_document_sender_person",
                table: "common_html_template",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "document_sender_person_id",
                table: "case_notification_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "document_sender_person_id",
                table: "case_notification",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "have_document_sender_person",
                table: "common_html_template");

            migrationBuilder.DropColumn(
                name: "document_sender_person_id",
                table: "case_notification_h");

            migrationBuilder.DropColumn(
                name: "document_sender_person_id",
                table: "case_notification");
        }
    }
}
