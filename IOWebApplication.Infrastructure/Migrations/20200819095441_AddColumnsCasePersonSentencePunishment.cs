using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCasePersonSentencePunishment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "probation_days",
                table: "case_person_sentence_punishment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "probation_months",
                table: "case_person_sentence_punishment",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "probation_start_date",
                table: "case_person_sentence_punishment",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "probation_weeks",
                table: "case_person_sentence_punishment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "probation_years",
                table: "case_person_sentence_punishment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "probation_days",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "probation_months",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "probation_start_date",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "probation_weeks",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "probation_years",
                table: "case_person_sentence_punishment");
        }
    }
}
