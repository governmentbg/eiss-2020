using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class SentenceType_IsEffective : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_probation",
                table: "nom_sentence_type",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_effective",
                table: "nom_sentence_type",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_probation",
                table: "nom_sentence_type");

            migrationBuilder.DropColumn(
                name: "is_effective",
                table: "nom_sentence_type");
        }
    }
}
