using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddLawyerStateDateInAssignedLawyer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lawyer_id",
                table: "case_lawyer_help_assigned_lawyer",
                newName: "lawyer_state_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_lawyer_id",
                table: "case_lawyer_help_assigned_lawyer",
                newName: "IX_case_lawyer_help_assigned_lawyer_lawyer_state_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "lawyer_state_date",
                table: "case_lawyer_help_assigned_lawyer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lawyer_state_date",
                table: "case_lawyer_help_assigned_lawyer");

            migrationBuilder.RenameColumn(
                name: "lawyer_state_id",
                table: "case_lawyer_help_assigned_lawyer",
                newName: "lawyer_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_lawyer_state_id",
                table: "case_lawyer_help_assigned_lawyer",
                newName: "IX_case_lawyer_help_assigned_lawyer_lawyer_id");
        }
    }
}
