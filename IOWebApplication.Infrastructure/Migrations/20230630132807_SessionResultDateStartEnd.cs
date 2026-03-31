using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class SessionResultDateStartEnd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_end",
                table: "nom_session_result_filter_rule",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_start",
                table: "nom_session_result_filter_rule",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_end",
                table: "nom_court_type_session_type",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_start",
                table: "nom_court_type_session_type",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_end",
                table: "nom_session_result_filter_rule");

            migrationBuilder.DropColumn(
                name: "date_start",
                table: "nom_session_result_filter_rule");

            migrationBuilder.DropColumn(
                name: "date_end",
                table: "nom_court_type_session_type");

            migrationBuilder.DropColumn(
                name: "date_start",
                table: "nom_court_type_session_type");
        }
    }
}
