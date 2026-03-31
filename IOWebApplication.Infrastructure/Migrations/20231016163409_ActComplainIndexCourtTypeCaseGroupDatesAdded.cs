using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ActComplainIndexCourtTypeCaseGroupDatesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_end",
                table: "nom_act_complain_index_court_type_case_group",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_start",
                table: "nom_act_complain_index_court_type_case_group",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_end",
                table: "nom_act_complain_index_court_type_case_group");

            migrationBuilder.DropColumn(
                name: "date_start",
                table: "nom_act_complain_index_court_type_case_group");
        }
    }
}
