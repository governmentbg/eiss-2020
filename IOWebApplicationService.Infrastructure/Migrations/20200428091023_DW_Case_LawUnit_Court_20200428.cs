using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_Case_LawUnit_Court_20200428 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "case_instance_code",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_instance_id",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "case_instance_name",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_code",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_id",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "court_name",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_region_id",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "court_region_name",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_type_id",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "court_type_name",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_transfered_dw",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_wrt",
                table: "dw_case_session_lawunit",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "eispp_code",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ecli_code",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parent_court_id",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_court_name",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "case_instance_code",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_instance_id",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "case_instance_name",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_code",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city_name",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_id",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "court_name",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_region_id",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "court_region_name",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_type_id",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "court_type_name",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_transfered_dw",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_wrt",
                table: "dw_case_lawunit",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "eispp_code",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ecli_code",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parent_court_id",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_court_name",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                table: "dw_case_lawunit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "case_instance_code",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "case_instance_id",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "case_instance_name",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "city_code",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "city_name",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "court_id",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "court_name",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "court_region_id",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "court_region_name",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "court_type_id",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "court_type_name",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "date_transfered_dw",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "date_wrt",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "eispp_code",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "ecli_code",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "parent_court_id",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "parent_court_name",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "user_name",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "case_instance_code",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "case_instance_id",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "case_instance_name",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "city_code",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "city_name",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "court_id",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "court_name",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "court_region_id",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "court_region_name",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "court_type_id",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "court_type_name",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "date_transfered_dw",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "date_wrt",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "eispp_code",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "ecli_code",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "parent_court_id",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "parent_court_name",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "user_name",
                table: "dw_case_lawunit");
        }
    }
}
