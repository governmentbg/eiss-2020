using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseNotificationActs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "have_session_multi_act",
                table: "common_html_template",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "case_notification_act",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_notification_id = table.Column<int>(type: "integer", nullable: false),
                    is_checked = table.Column<bool>(type: "boolean", nullable: false),
                    case_session_act_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_notification_act", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_notification_act_case_notification_case_notification_id",
                        column: x => x.case_notification_id,
                        principalTable: "case_notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_notification_act_case_session_act_case_session_act_id",
                        column: x => x.case_session_act_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_notification_act_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_act_case_notification_id",
                table: "case_notification_act",
                column: "case_notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_act_case_session_act_id",
                table: "case_notification_act",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_act_user_id",
                table: "case_notification_act",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_notification_act");

            migrationBuilder.DropColumn(
                name: "have_session_multi_act",
                table: "common_html_template");
        }
    }
}
