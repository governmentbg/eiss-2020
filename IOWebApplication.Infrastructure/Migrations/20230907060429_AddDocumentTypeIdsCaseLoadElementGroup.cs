using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddDocumentTypeIdsCaseLoadElementGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "document_type_ids",
                table: "nom_case_load_element_group",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "document_type_ids",
                table: "nom_case_load_element_group");
        }
    }
}
