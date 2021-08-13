using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CasePersonLink_Expired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "case_person_link",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "case_person_link",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "case_person_link",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "case_person_link");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "case_person_link");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "case_person_link");
        }
    }
}
