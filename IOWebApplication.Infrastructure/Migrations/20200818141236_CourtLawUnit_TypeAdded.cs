using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CourtLawUnit_TypeAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "law_unit_type_id",
                table: "common_court_lawunit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_law_unit_type_id",
                table: "common_court_lawunit",
                column: "law_unit_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_court_lawunit_nom_law_unit_type_law_unit_type_id",
                table: "common_court_lawunit",
                column: "law_unit_type_id",
                principalTable: "nom_law_unit_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_court_lawunit_nom_law_unit_type_law_unit_type_id",
                table: "common_court_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_common_court_lawunit_law_unit_type_id",
                table: "common_court_lawunit");

            migrationBuilder.DropColumn(
                name: "law_unit_type_id",
                table: "common_court_lawunit");
        }
    }
}
