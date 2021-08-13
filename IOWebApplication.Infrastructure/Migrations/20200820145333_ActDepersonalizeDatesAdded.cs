using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ActDepersonalizeDatesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "depersonalize_end_date",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "depersonalize_start_date",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "depersonalize_end_date",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "depersonalize_start_date",
                table: "case_session_act",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "depersonalize_end_date",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "depersonalize_start_date",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "depersonalize_end_date",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "depersonalize_start_date",
                table: "case_session_act");
        }
    }
}
