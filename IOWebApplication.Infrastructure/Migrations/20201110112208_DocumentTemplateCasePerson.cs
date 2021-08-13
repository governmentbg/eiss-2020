using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentTemplateCasePerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_person_address_id",
                table: "document_template",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_person_id",
                table: "document_template",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "have_case_person",
                table: "common_html_template",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_template_case_person_address_id",
                table: "document_template",
                column: "case_person_address_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_template_case_person_id",
                table: "document_template",
                column: "case_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_template_case_person_address_case_person_address_id",
                table: "document_template",
                column: "case_person_address_id",
                principalTable: "case_person_address",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_document_template_case_person_case_person_id",
                table: "document_template",
                column: "case_person_id",
                principalTable: "case_person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_template_case_person_address_case_person_address_id",
                table: "document_template");

            migrationBuilder.DropForeignKey(
                name: "FK_document_template_case_person_case_person_id",
                table: "document_template");

            migrationBuilder.DropIndex(
                name: "IX_document_template_case_person_address_id",
                table: "document_template");

            migrationBuilder.DropIndex(
                name: "IX_document_template_case_person_id",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "case_person_address_id",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "case_person_id",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "have_case_person",
                table: "common_html_template");
        }
    }
}
