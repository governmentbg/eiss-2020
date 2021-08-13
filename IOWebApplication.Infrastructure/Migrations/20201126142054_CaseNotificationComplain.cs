using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseNotificationComplain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_notification_complain",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    case_notification_id = table.Column<int>(nullable: false),
                    case_session_act_complain_id = table.Column<int>(nullable: true),
                    is_checked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_notification_complain", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_notification_complain_case_notification_case_notificat~",
                        column: x => x.case_notification_id,
                        principalTable: "case_notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_notification_complain_case_session_act_complain_case_s~",
                        column: x => x.case_session_act_complain_id,
                        principalTable: "case_session_act_complain",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_notification_complain_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_complain_case_notification_id",
                table: "case_notification_complain",
                column: "case_notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_complain_case_session_act_complain_id",
                table: "case_notification_complain",
                column: "case_session_act_complain_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_complain_user_id",
                table: "case_notification_complain",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_notification_complain");
        }
    }
}
