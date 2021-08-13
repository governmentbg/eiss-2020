using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_Case_LawUnit20200427 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_session_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dw_count",
                table: "dw_case_lawunit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_session_lawunit");

            migrationBuilder.DropColumn(
                name: "dw_count",
                table: "dw_case_lawunit");
        }
    }
}
