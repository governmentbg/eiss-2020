using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddFieldForReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "declared_month_count",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "declared_month_count",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "to_now_duration_months",
                table: "case_lifecycle",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "update_date_to_now_duration_months",
                table: "case_lifecycle",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "declared_month_count",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "declared_month_count",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "to_now_duration_months",
                table: "case_lifecycle");

            migrationBuilder.DropColumn(
                name: "update_date_to_now_duration_months",
                table: "case_lifecycle");
        }
    }
}
