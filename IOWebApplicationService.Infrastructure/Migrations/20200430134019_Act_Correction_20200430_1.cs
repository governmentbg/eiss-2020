using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class Act_Correction_20200430_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "dw_case_session_result");

            migrationBuilder.DropColumn(
                name: "session_date_expired",
                table: "dw_case_session_result");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_result",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_lawunit",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired",
                table: "dw_case_session_lawunit",
                newName: "date_expired");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_divorce",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_divorce",
                newName: "date_expired");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_coordination",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_coordination",
                newName: "date_expired");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain_result",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_complain_result",
                newName: "date_expired");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain_person",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_complain_person",
                newName: "date_expired");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act_complain",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired",
                table: "dw_case_session_act_complain",
                newName: "date_expired");

            migrationBuilder.RenameColumn(
                name: "session_date_expired_str",
                table: "dw_case_session_act",
                newName: "date_expired_str");

            migrationBuilder.RenameColumn(
                name: "session_date_expired",
                table: "dw_case_session_act",
                newName: "date_expired");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_result",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_lawunit",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired",
                table: "dw_case_session_lawunit",
                newName: "session_date_expired");

            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_act_divorce",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired",
                table: "dw_case_session_act_divorce",
                newName: "session_date_expired");

            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_act_coordination",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired",
                table: "dw_case_session_act_coordination",
                newName: "session_date_expired");

            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_act_complain_result",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired",
                table: "dw_case_session_act_complain_result",
                newName: "session_date_expired");

            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_act_complain_person",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired",
                table: "dw_case_session_act_complain_person",
                newName: "session_date_expired");

            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_act_complain",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired",
                table: "dw_case_session_act_complain",
                newName: "session_date_expired");

            migrationBuilder.RenameColumn(
                name: "date_expired_str",
                table: "dw_case_session_act",
                newName: "session_date_expired_str");

            migrationBuilder.RenameColumn(
                name: "date_expired",
                table: "dw_case_session_act",
                newName: "session_date_expired");

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "dw_case_session_result",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "session_date_expired",
                table: "dw_case_session_result",
                nullable: true);
        }
    }
}
