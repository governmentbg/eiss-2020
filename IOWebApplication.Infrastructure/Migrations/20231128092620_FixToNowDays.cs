using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class FixToNowDays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "to_now_duration_months",
                table: "case_lifecycle");

            migrationBuilder.DropColumn(
                name: "update_date_to_now_duration_months",
                table: "case_lifecycle");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
