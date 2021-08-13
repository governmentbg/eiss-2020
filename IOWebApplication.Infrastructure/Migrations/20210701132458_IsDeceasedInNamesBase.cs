using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class IsDeceasedInNamesBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "money_obligation_receive",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "money_obligation_receive",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "money_obligation",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "money_obligation",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "document_person",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "common_person",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "common_person",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "common_law_unit_h",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "common_law_unit_h",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "common_law_unit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "common_law_unit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "common_institution",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "common_institution",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "case_person_h",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deceased",
                table: "case_person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "money_obligation_receive");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "money_obligation_receive");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "document_person");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "common_person");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "common_person");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "common_law_unit_h");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "common_law_unit_h");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "common_law_unit");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "common_law_unit");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "common_institution");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "common_institution");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "date_deceased",
                table: "case_person");
        }
    }
}
