using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumenttypeDefaultHtmlTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "default_html_template_id",
                table: "nom_document_type",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_nom_document_type_default_html_template_id",
                table: "nom_document_type",
                column: "default_html_template_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_document_type_common_html_template_default_html_templat~",
                table: "nom_document_type",
                column: "default_html_template_id",
                principalTable: "common_html_template",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_document_type_common_html_template_default_html_templat~",
                table: "nom_document_type");

            migrationBuilder.DropIndex(
                name: "IX_nom_document_type_default_html_template_id",
                table: "nom_document_type");

            migrationBuilder.DropColumn(
                name: "default_html_template_id",
                table: "nom_document_type");
        }
    }
}
