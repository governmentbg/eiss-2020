using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class Act_Correction_20200430 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_result",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_result",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_act_coordination",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_act_coordination",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_act_complain_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain_person",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "document_date",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_date_str",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "document_number",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_date_expired_str",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_expired_str",
                table: "dw_case_session",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "case_code_full_object",
                table: "dw_case",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "case_code_lawbase_description",
                table: "dw_case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_result");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_result");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_coordination");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_coordination");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_complain_person");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain_person");

            migrationBuilder.DropColumn(
                name: "document_date",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "document_date_str",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "document_number",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "date_expired_str",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "case_code_full_object",
                table: "dw_case");

            migrationBuilder.DropColumn(
                name: "case_code_lawbase_description",
                table: "dw_case");
        }
    }
}
