using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class FieldsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "date_from_str",
                table: "dw_case_session",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_to_str",
                table: "dw_case_session",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_from_str",
                table: "dw_case_session");

            migrationBuilder.DropColumn(
                name: "date_to_str",
                table: "dw_case_session");
        }
    }
}
