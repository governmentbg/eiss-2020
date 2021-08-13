using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsLawBaseNew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_group_id",
                table: "nom_law_base",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_instance_id",
                table: "nom_law_base",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_type_id",
                table: "nom_law_base",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_nom_law_base_case_group_id",
                table: "nom_law_base",
                column: "case_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_law_base_case_instance_id",
                table: "nom_law_base",
                column: "case_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_law_base_court_type_id",
                table: "nom_law_base",
                column: "court_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_law_base_nom_case_group_case_group_id",
                table: "nom_law_base",
                column: "case_group_id",
                principalTable: "nom_case_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_law_base_nom_case_instance_case_instance_id",
                table: "nom_law_base",
                column: "case_instance_id",
                principalTable: "nom_case_instance",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_law_base_nom_court_type_court_type_id",
                table: "nom_law_base",
                column: "court_type_id",
                principalTable: "nom_court_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_law_base_nom_case_group_case_group_id",
                table: "nom_law_base");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_law_base_nom_case_instance_case_instance_id",
                table: "nom_law_base");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_law_base_nom_court_type_court_type_id",
                table: "nom_law_base");

            migrationBuilder.DropIndex(
                name: "IX_nom_law_base_case_group_id",
                table: "nom_law_base");

            migrationBuilder.DropIndex(
                name: "IX_nom_law_base_case_instance_id",
                table: "nom_law_base");

            migrationBuilder.DropIndex(
                name: "IX_nom_law_base_court_type_id",
                table: "nom_law_base");

            migrationBuilder.DropColumn(
                name: "case_group_id",
                table: "nom_law_base");

            migrationBuilder.DropColumn(
                name: "case_instance_id",
                table: "nom_law_base");

            migrationBuilder.DropColumn(
                name: "court_type_id",
                table: "nom_law_base");
        }
    }
}
