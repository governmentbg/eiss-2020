using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DismissalFKAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_dismisal_document_id",
                table: "case_lawunit_dismisal",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_dismisal_document_person_id",
                table: "case_lawunit_dismisal",
                column: "document_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_dismisal_document_document_id",
                table: "case_lawunit_dismisal",
                column: "document_id",
                principalTable: "document",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_dismisal_document_person_document_person_id",
                table: "case_lawunit_dismisal",
                column: "document_person_id",
                principalTable: "document_person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_dismisal_document_document_id",
                table: "case_lawunit_dismisal");

            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_dismisal_document_person_document_person_id",
                table: "case_lawunit_dismisal");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_dismisal_document_id",
                table: "case_lawunit_dismisal");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_dismisal_document_person_id",
                table: "case_lawunit_dismisal");
        }
    }
}
