using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_CaseSessionActDivorce20200420 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "birth_day_man",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "birth_day_woman",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "case_id",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_person_man_id",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "case_person_man_name",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_person_woman_id",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "case_person_woman_name",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_session_act_id",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "children_over_18",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "children_under_18",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "country_code_date",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "divorce_count_man",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "divorce_count_woman",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "education_man",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "education_woman",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marriage_count_man",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "marriage_count_woman",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "marriage_date",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "marriage_fault",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marriage_fault_description",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marriage_number",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marriage_place",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "married_status_before_man",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "married_status_before_woman",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_after_marriage_man",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name_after_marriage_woman",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nationality_man",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nationality_woman",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "out_document_id",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reg_date",
                table: "dw_case_session_act_divorce",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "reg_number",
                table: "dw_case_session_act_divorce",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birth_day_man",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "birth_day_woman",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "case_id",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "case_person_man_id",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "case_person_man_name",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "case_person_woman_id",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "case_person_woman_name",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "case_session_act_id",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "children_over_18",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "children_under_18",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "country_code_date",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "divorce_count_man",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "divorce_count_woman",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "education_man",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "education_woman",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "id",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "marriage_count_man",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "marriage_count_woman",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "marriage_date",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "marriage_fault",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "marriage_fault_description",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "marriage_number",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "marriage_place",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "married_status_before_man",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "married_status_before_woman",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "name_after_marriage_man",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "name_after_marriage_woman",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "nationality_man",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "nationality_woman",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "out_document_id",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "reg_date",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "reg_number",
                table: "dw_case_session_act_divorce");
        }
    }
}
