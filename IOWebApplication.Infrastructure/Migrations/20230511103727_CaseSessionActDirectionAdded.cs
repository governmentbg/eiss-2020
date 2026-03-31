using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseSessionActDirectionAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "act_direction_id",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "act_direction_id",
                table: "case_session_act",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "act_direction_id",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "act_direction_id",
                table: "case_session_act");
        }
    }
}
