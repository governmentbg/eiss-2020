using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_Case_SessionActComplain_20200429_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_session_id",
                table: "dw_case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_id",
                table: "dw_case_session_act_complain_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_session_id",
                table: "dw_case_session_act_complain_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_session_id",
                table: "dw_case_session_act_complain",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "case_session_id",
                table: "dw_case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "case_id",
                table: "dw_case_session_act_complain_person");

            migrationBuilder.DropColumn(
                name: "case_session_id",
                table: "dw_case_session_act_complain_person");

            migrationBuilder.DropColumn(
                name: "case_session_id",
                table: "dw_case_session_act_complain");
        }
    }
}
