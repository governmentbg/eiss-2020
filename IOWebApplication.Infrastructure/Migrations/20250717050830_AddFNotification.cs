using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "initial_date",
                table: "common_work_notification",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_days",
                table: "case_session_act_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_months",
                table: "case_session_act_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "notification_on",
                table: "case_session_act_h",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_weeks",
                table: "case_session_act_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_days",
                table: "case_session_act",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_months",
                table: "case_session_act",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "notification_on",
                table: "case_session_act",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_weeks",
                table: "case_session_act",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "initial_date",
                table: "common_work_notification");

            migrationBuilder.DropColumn(
                name: "notification_days",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "notification_months",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "notification_on",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "notification_weeks",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "notification_days",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "notification_months",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "notification_on",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "notification_weeks",
                table: "case_session_act");
        }
    }
}
