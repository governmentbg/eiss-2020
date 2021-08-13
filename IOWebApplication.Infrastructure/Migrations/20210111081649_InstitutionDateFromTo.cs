using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class InstitutionDateFromTo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_from",
                table: "common_institution",
                nullable: false,
                defaultValueSql: "make_date(2000,1,1)");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_to",
                table: "common_institution",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_from",
                table: "common_institution");

            migrationBuilder.DropColumn(
                name: "date_to",
                table: "common_institution");
        }
    }
}
