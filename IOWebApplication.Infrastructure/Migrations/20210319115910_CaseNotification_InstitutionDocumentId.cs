using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseNotification_InstitutionDocumentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "institution_document_id",
                table: "case_notification_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_obligation_id",
                table: "case_notification_h",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "institution_document_id",
                table: "case_notification",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_obligation_id",
                table: "case_notification",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "institution_document_id",
                table: "case_notification_h");

            migrationBuilder.DropColumn(
                name: "money_obligation_id",
                table: "case_notification_h");

            migrationBuilder.DropColumn(
                name: "institution_document_id",
                table: "case_notification");

            migrationBuilder.DropColumn(
                name: "money_obligation_id",
                table: "case_notification");
        }
    }
}
