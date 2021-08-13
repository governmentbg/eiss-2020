using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class TaskPersonDocAlter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "multi_registration_id",
                table: "document",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parent_description",
                table: "common_worktask",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "case_person_h",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deceased",
                table: "case_person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "multi_registration_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "parent_description",
                table: "common_worktask");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "is_deceased",
                table: "case_person");
        }
    }
}
