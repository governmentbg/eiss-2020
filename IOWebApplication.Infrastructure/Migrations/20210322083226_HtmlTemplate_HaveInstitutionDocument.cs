using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class HtmlTemplate_HaveInstitutionDocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "have_institution_document",
                table: "common_html_template",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "have_money_obligation",
                table: "common_html_template",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "have_institution_document",
                table: "common_html_template");

            migrationBuilder.DropColumn(
                name: "have_money_obligation",
                table: "common_html_template");
        }
    }
}
