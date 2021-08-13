using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_Session20200503 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "judge_reporter_id",
                table: "dw_case_session",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "judge_reporter_name",
                table: "dw_case_session",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_full_judge_staff",
                table: "dw_case_session",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_full_staff",
                table: "dw_case_session",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_judge_staff",
                table: "dw_case_session",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "session_juri_staff",
                table: "dw_case_session",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "judge_reporter_id",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "judge_reporter_name",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "session_full_judge_staff",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "session_full_staff",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "session_judge_staff",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "session_juri_staff",
                table: "dw_case_session");
        }
    }
}
