using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentNotification_DocumentPersonLinkId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_notification_document_person_link_case_person_link~",
                table: "document_notification");

            migrationBuilder.RenameColumn(
                name: "case_person_link_id",
                table: "document_notification",
                newName: "document_person_link_id");

            migrationBuilder.RenameIndex(
                name: "IX_document_notification_case_person_link_id",
                table: "document_notification",
                newName: "IX_document_notification_document_person_link_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_notification_document_person_link_document_person_~",
                table: "document_notification",
                column: "document_person_link_id",
                principalTable: "document_person_link",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_notification_document_person_link_document_person_~",
                table: "document_notification");

            migrationBuilder.RenameColumn(
                name: "document_person_link_id",
                table: "document_notification",
                newName: "case_person_link_id");

            migrationBuilder.RenameIndex(
                name: "IX_document_notification_document_person_link_id",
                table: "document_notification",
                newName: "IX_document_notification_case_person_link_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_notification_document_person_link_case_person_link~",
                table: "document_notification",
                column: "case_person_link_id",
                principalTable: "document_person_link",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
