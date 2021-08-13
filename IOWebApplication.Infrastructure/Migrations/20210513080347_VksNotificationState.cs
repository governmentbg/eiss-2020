using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VksNotificationState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_person_link_id",
                table: "case_session_notification_list",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "vks_notification_state_id",
                table: "case_session_notification_list",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_vks_notification_state",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    order_number = table.Column<int>(nullable: false),
                    code = table.Column<string>(nullable: true),
                    label = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    date_start = table.Column<DateTime>(nullable: false),
                    date_end = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_vks_notification_state", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_session_notification_list_case_person_link_id",
                table: "case_session_notification_list",
                column: "case_person_link_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_notification_list_vks_notification_state_id",
                table: "case_session_notification_list",
                column: "vks_notification_state_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_notification_list_case_person_link_case_person~",
                table: "case_session_notification_list",
                column: "case_person_link_id",
                principalTable: "case_person_link",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_notification_list_nom_vks_notification_state_v~",
                table: "case_session_notification_list",
                column: "vks_notification_state_id",
                principalTable: "nom_vks_notification_state",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_notification_list_case_person_link_case_person~",
                table: "case_session_notification_list");

            migrationBuilder.DropForeignKey(
                name: "FK_case_session_notification_list_nom_vks_notification_state_v~",
                table: "case_session_notification_list");

            migrationBuilder.DropTable(
                name: "nom_vks_notification_state");

            migrationBuilder.DropIndex(
                name: "IX_case_session_notification_list_case_person_link_id",
                table: "case_session_notification_list");

            migrationBuilder.DropIndex(
                name: "IX_case_session_notification_list_vks_notification_state_id",
                table: "case_session_notification_list");

            migrationBuilder.DropColumn(
                name: "case_person_link_id",
                table: "case_session_notification_list");

            migrationBuilder.DropColumn(
                name: "vks_notification_state_id",
                table: "case_session_notification_list");
        }
    }
}
