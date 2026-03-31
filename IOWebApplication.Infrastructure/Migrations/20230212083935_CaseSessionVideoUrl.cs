using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseSessionVideoUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "video_url",
                table: "case_session_h",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_url",
                table: "case_session",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "video_url",
                table: "case_session_h");

            migrationBuilder.DropColumn(
                name: "video_url",
                table: "case_session");
        }
    }
}
