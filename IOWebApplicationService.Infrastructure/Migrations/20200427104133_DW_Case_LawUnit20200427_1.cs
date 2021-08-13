using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_Case_LawUnit20200427_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "date_from_str",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_to_str",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_from_str",
                table: "dw_case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_to_str",
                table: "dw_case_lawunit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_from_str",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "date_to_str",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "date_from_str",
                table: "dw_case_lawunit");

            migrationBuilder.DropColumn(
                name: "date_to_str",
                table: "dw_case_lawunit");
        }
    }
}
