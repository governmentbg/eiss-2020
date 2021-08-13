using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_Case_SessionAct_20200428 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "act_date_str",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "act_declared_date_str",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "act_inforced_date_str",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "act_motives_declared_date_str",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_id",
                table: "dw_case_session_act",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "reg_date_str",
                table: "dw_case_session_act",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "act_date_str",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "act_declared_date_str",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "act_inforced_date_str",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "act_motives_declared_date_str",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "case_id",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "reg_date_str",
                table: "dw_case_session_act");
        }
    }
}
