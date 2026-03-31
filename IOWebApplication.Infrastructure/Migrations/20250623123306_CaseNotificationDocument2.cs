using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseNotificationDocument2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseNotificationDocuments_case_notification_case_notificati~",
                table: "CaseNotificationDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseNotificationDocuments_document_document_id",
                table: "CaseNotificationDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseNotificationDocuments_identity_users_user_id",
                table: "CaseNotificationDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CaseNotificationDocuments",
                table: "CaseNotificationDocuments");

            migrationBuilder.RenameTable(
                name: "CaseNotificationDocuments",
                newName: "case_notification_document");

            migrationBuilder.RenameIndex(
                name: "IX_CaseNotificationDocuments_user_id",
                table: "case_notification_document",
                newName: "IX_case_notification_document_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_CaseNotificationDocuments_document_id",
                table: "case_notification_document",
                newName: "IX_case_notification_document_document_id");

            migrationBuilder.RenameIndex(
                name: "IX_CaseNotificationDocuments_case_notification_id",
                table: "case_notification_document",
                newName: "IX_case_notification_document_case_notification_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_case_notification_document",
                table: "case_notification_document",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_notification_document_case_notification_case_notificat~",
                table: "case_notification_document",
                column: "case_notification_id",
                principalTable: "case_notification",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_case_notification_document_document_document_id",
                table: "case_notification_document",
                column: "document_id",
                principalTable: "document",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_case_notification_document_identity_users_user_id",
                table: "case_notification_document",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_notification_document_case_notification_case_notificat~",
                table: "case_notification_document");

            migrationBuilder.DropForeignKey(
                name: "FK_case_notification_document_document_document_id",
                table: "case_notification_document");

            migrationBuilder.DropForeignKey(
                name: "FK_case_notification_document_identity_users_user_id",
                table: "case_notification_document");

            migrationBuilder.DropPrimaryKey(
                name: "PK_case_notification_document",
                table: "case_notification_document");

            migrationBuilder.RenameTable(
                name: "case_notification_document",
                newName: "CaseNotificationDocuments");

            migrationBuilder.RenameIndex(
                name: "IX_case_notification_document_user_id",
                table: "CaseNotificationDocuments",
                newName: "IX_CaseNotificationDocuments_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_notification_document_document_id",
                table: "CaseNotificationDocuments",
                newName: "IX_CaseNotificationDocuments_document_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_notification_document_case_notification_id",
                table: "CaseNotificationDocuments",
                newName: "IX_CaseNotificationDocuments_case_notification_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CaseNotificationDocuments",
                table: "CaseNotificationDocuments",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseNotificationDocuments_case_notification_case_notificati~",
                table: "CaseNotificationDocuments",
                column: "case_notification_id",
                principalTable: "case_notification",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseNotificationDocuments_document_document_id",
                table: "CaseNotificationDocuments",
                column: "document_id",
                principalTable: "document",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseNotificationDocuments_identity_users_user_id",
                table: "CaseNotificationDocuments",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id");
        }
    }
}
