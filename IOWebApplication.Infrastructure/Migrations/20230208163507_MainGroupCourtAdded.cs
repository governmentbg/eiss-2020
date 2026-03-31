using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class MainGroupCourtAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "court_id",
                table: "common_main_group",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_common_main_group_court_id",
                table: "common_main_group",
                column: "court_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_main_group_common_court_court_id",
                table: "common_main_group",
                column: "court_id",
                principalTable: "common_court",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_main_group_common_court_court_id",
                table: "common_main_group");

            migrationBuilder.DropIndex(
                name: "IX_common_main_group_court_id",
                table: "common_main_group");

            migrationBuilder.DropColumn(
                name: "court_id",
                table: "common_main_group");
        }
    }
}
