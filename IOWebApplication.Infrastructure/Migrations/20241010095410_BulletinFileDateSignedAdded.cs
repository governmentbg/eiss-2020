using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BulletinFileDateSignedAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date_export",
                table: "case_person_sentence_bulletin_export",
                newName: "date_submited");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_registered_in_cais",
                table: "case_person_sentence_bulletin_export",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_signed",
                table: "case_person_sentence_bulletin_export",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_registered_in_cais",
                table: "case_person_sentence_bulletin_export");

            migrationBuilder.DropColumn(
                name: "date_signed",
                table: "case_person_sentence_bulletin_export");

            migrationBuilder.RenameColumn(
                name: "date_submited",
                table: "case_person_sentence_bulletin_export",
                newName: "date_export");
        }
    }
}
