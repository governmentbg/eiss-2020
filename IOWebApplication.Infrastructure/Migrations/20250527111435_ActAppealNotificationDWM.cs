using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActAppealNotificationDWM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "appeal_notification_days_fast_process",
                table: "case_session_act_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "appeal_notification_days_months_process",
                table: "case_session_act_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "appeal_notification_weeks_fast_process",
                table: "case_session_act_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "appeal_notification_days_fast_process",
                table: "case_session_act",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "appeal_notification_days_months_process",
                table: "case_session_act",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "appeal_notification_weeks_fast_process",
                table: "case_session_act",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "appeal_notification_days_fast_process",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "appeal_notification_days_months_process",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "appeal_notification_weeks_fast_process",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "appeal_notification_days_fast_process",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "appeal_notification_days_months_process",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "appeal_notification_weeks_fast_process",
                table: "case_session_act");
        }
    }
}
