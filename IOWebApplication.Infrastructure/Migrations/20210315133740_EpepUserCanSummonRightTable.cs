using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EpepUserCanSummonRightTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_summon",
                table: "epep_user");

            migrationBuilder.AddColumn<bool>(
                name: "can_summon",
                table: "epep_user_assignment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_summon",
                table: "epep_user_assignment");

            migrationBuilder.AddColumn<bool>(
                name: "can_summon",
                table: "epep_user",
                nullable: true);
        }
    }
}
