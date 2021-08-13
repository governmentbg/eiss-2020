using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_ActComplain20200519 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "out_complain_document_id",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "out_document_name",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "out_document_type_id",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "out_document_type_name",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "out_court_id",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "out_court_name",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "out_document_date",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "out_document_date_str",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "out_document_number",
                table: "dw_case_session_act_complain",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "out_complain_document_id",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_document_name",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_document_type_id",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_document_type_name",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_court_id",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_court_name",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_document_date",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_document_date_str",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "out_document_number",
                table: "dw_case_session_act_complain");
        }
    }
}
