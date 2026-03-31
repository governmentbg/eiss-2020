using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseIsRenewCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_renew_case",
                table: "case_h",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_renew_case",
                table: "case",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_renew_case",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "is_renew_case",
                table: "case");
        }
    }
}
