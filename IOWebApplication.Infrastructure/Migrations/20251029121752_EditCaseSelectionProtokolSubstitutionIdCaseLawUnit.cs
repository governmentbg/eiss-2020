using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditCaseSelectionProtokolSubstitutionIdCaseLawUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_selection_protokol_substitution_id",
                table: "case_lawunit",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на протокол за разпределяне извън дело");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_case_selection_protokol_substitution_id",
                table: "case_lawunit",
                column: "case_selection_protokol_substitution_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_case_selection_protokol_substitution_case_sele~",
                table: "case_lawunit",
                column: "case_selection_protokol_substitution_id",
                principalTable: "case_selection_protokol_substitution",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_case_selection_protokol_substitution_case_sele~",
                table: "case_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_case_selection_protokol_substitution_id",
                table: "case_lawunit");

            migrationBuilder.DropColumn(
                name: "case_selection_protokol_substitution_id",
                table: "case_lawunit");
        }
    }
}
