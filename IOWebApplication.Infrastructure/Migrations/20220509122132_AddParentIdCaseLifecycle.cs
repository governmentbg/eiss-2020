using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddParentIdCaseLifecycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_id",
                table: "case_lifecycle",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_lifecycle_parent_id",
                table: "case_lifecycle",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lifecycle_case_lifecycle_parent_id",
                table: "case_lifecycle",
                column: "parent_id",
                principalTable: "case_lifecycle",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lifecycle_case_lifecycle_parent_id",
                table: "case_lifecycle");

            migrationBuilder.DropIndex(
                name: "IX_case_lifecycle_parent_id",
                table: "case_lifecycle");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "case_lifecycle");
        }
    }
}
