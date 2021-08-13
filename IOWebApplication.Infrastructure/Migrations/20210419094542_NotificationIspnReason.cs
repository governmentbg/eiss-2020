using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class NotificationIspnReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "notification_ispn_reason_id",
                table: "case_notification_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_ispn_reason_id",
                table: "case_notification",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_notification_act_ispn_reason",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    code = table.Column<string>(nullable: true),
                    label = table.Column<string>(nullable: true),
                    accomply = table.Column<string>(nullable: true),
                    meeting_type = table.Column<string>(nullable: true),
                    meeting_agenda = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_notification_act_ispn_reason", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_notification_notification_ispn_reason_id",
                table: "case_notification",
                column: "notification_ispn_reason_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_notification_nom_notification_act_ispn_reason_notifica~",
                table: "case_notification",
                column: "notification_ispn_reason_id",
                principalTable: "nom_notification_act_ispn_reason",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_notification_nom_notification_act_ispn_reason_notifica~",
                table: "case_notification");

            migrationBuilder.DropTable(
                name: "nom_notification_act_ispn_reason");

            migrationBuilder.DropIndex(
                name: "IX_case_notification_notification_ispn_reason_id",
                table: "case_notification");

            migrationBuilder.DropColumn(
                name: "notification_ispn_reason_id",
                table: "case_notification_h");

            migrationBuilder.DropColumn(
                name: "notification_ispn_reason_id",
                table: "case_notification");
        }
    }
}
