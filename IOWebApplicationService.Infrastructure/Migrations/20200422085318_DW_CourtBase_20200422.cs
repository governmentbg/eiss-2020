using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_CourtBase_20200422 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_document_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_document_link",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_document_institution_case_info",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_document_decision_case",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_document_decision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_document_case_info",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_document",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_result",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_act_coordination",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_act_complain_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_selection_protocol_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_selection_protocol_compartment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_selection_protocol",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_document_person");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_document_link");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_document_institution_case_info");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_document_decision_case");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_document_decision");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_document_case_info");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_document");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_result");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_act_coordination");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_act_complain_person");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_selection_protocol_lawunit");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_selection_protocol_compartment");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_selection_protocol");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case");
        }
    }
}
