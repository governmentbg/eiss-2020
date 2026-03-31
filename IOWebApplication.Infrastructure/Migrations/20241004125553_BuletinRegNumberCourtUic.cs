using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BuletinRegNumberCourtUic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nick_name",
                table: "case_person_sentence_bulletin",
                newName: "reg_number");

            migrationBuilder.AddColumn<string>(
                name: "uic",
                table: "common_court",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_generated_date",
                table: "case_person_sentence_bulletin",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reg_date",
                table: "case_person_sentence_bulletin",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "uic",
                table: "common_court");

            migrationBuilder.DropColumn(
                name: "last_generated_date",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "reg_date",
                table: "case_person_sentence_bulletin");

            migrationBuilder.RenameColumn(
                name: "reg_number",
                table: "case_person_sentence_bulletin",
                newName: "nick_name");
        }
    }
}
