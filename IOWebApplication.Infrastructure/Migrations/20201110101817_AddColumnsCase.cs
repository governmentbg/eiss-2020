using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "judical_composition_id",
                table: "case_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "otdelenie_id",
                table: "case_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "judical_composition_id",
                table: "case",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "otdelenie_id",
                table: "case",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_judical_composition_id",
                table: "case",
                column: "judical_composition_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_otdelenie_id",
                table: "case",
                column: "otdelenie_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_common_court_department_judical_composition_id",
                table: "case",
                column: "judical_composition_id",
                principalTable: "common_court_department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_common_court_department_otdelenie_id",
                table: "case",
                column: "otdelenie_id",
                principalTable: "common_court_department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_common_court_department_judical_composition_id",
                table: "case");

            migrationBuilder.DropForeignKey(
                name: "FK_case_common_court_department_otdelenie_id",
                table: "case");

            migrationBuilder.DropIndex(
                name: "IX_case_judical_composition_id",
                table: "case");

            migrationBuilder.DropIndex(
                name: "IX_case_otdelenie_id",
                table: "case");

            migrationBuilder.DropColumn(
                name: "judical_composition_id",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "otdelenie_id",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "judical_composition_id",
                table: "case");

            migrationBuilder.DropColumn(
                name: "otdelenie_id",
                table: "case");
        }
    }
}
