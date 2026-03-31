using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseNotificationDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "have_documents",
                table: "common_html_template",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "have_mongo_files",
                table: "common_html_template",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CaseNotificationDocuments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_notification_id = table.Column<int>(type: "integer", nullable: false),
                    document_id = table.Column<long>(type: "bigint", nullable: false),
                    is_checked = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseNotificationDocuments", x => x.id);
                    table.ForeignKey(
                        name: "FK_CaseNotificationDocuments_case_notification_case_notificati~",
                        column: x => x.case_notification_id,
                        principalTable: "case_notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseNotificationDocuments_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseNotificationDocuments_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotificationDocuments_case_notification_id",
                table: "CaseNotificationDocuments",
                column: "case_notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotificationDocuments_document_id",
                table: "CaseNotificationDocuments",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotificationDocuments_user_id",
                table: "CaseNotificationDocuments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseNotificationDocuments");

            migrationBuilder.DropColumn(
                name: "have_documents",
                table: "common_html_template");

            migrationBuilder.DropColumn(
                name: "have_mongo_files",
                table: "common_html_template");
        }
    }
}
