using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CasePersonRNFLalter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "appoint_date_from",
                table: "case_person_h",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "appoint_date_to",
                table: "case_person_h",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "birth_date",
                table: "case_person_h",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "related_act_id",
                table: "case_person_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "appoint_date_from",
                table: "case_person",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "appoint_date_to",
                table: "case_person",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "birth_date",
                table: "case_person",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "related_act_id",
                table: "case_person",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_person_related_act_id",
                table: "case_person",
                column: "related_act_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_case_session_act_related_act_id",
                table: "case_person",
                column: "related_act_id",
                principalTable: "case_session_act",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_case_session_act_related_act_id",
                table: "case_person");

            migrationBuilder.DropIndex(
                name: "IX_case_person_related_act_id",
                table: "case_person");

            migrationBuilder.DropColumn(
                name: "appoint_date_from",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "appoint_date_to",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "related_act_id",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "appoint_date_from",
                table: "case_person");

            migrationBuilder.DropColumn(
                name: "appoint_date_to",
                table: "case_person");

            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "case_person");

            migrationBuilder.DropColumn(
                name: "related_act_id",
                table: "case_person");
        }
    }
}
