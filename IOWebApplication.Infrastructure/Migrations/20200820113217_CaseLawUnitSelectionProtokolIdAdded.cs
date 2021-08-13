using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseLawUnitSelectionProtokolIdAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_selection_protokol_id",
                table: "case_lawunit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_case_selection_protokol_id",
                table: "case_lawunit",
                column: "case_selection_protokol_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_case_selection_protokol_case_selection_protoko~",
                table: "case_lawunit",
                column: "case_selection_protokol_id",
                principalTable: "case_selection_protokol",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_case_selection_protokol_case_selection_protoko~",
                table: "case_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_case_selection_protokol_id",
                table: "case_lawunit");

            migrationBuilder.DropColumn(
                name: "case_selection_protokol_id",
                table: "case_lawunit");
        }
    }
}
