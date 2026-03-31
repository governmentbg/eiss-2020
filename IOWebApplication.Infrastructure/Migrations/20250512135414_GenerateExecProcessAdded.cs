using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GenerateExecProcessAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_signed",
                table: "money_exec_list",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "generate_exec_process",
                table: "money_exec_list",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "generate_exec_process",
                table: "case_session_act_h",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "generate_exec_process",
                table: "case_session_act",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_signed",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "generate_exec_process",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "generate_exec_process",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "generate_exec_process",
                table: "case_session_act");
        }
    }
}
