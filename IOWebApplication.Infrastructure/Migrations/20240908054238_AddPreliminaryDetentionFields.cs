using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreliminaryDetentionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_preliminary_detention",
                table: "nom_sentence_type",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preliminary_detention_days",
                table: "case_person_sentence_punishment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "preliminary_detention_months",
                table: "case_person_sentence_punishment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "preliminary_detention_weeks",
                table: "case_person_sentence_punishment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "preliminary_detention_years",
                table: "case_person_sentence_punishment",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_preliminary_detention",
                table: "nom_sentence_type");

            migrationBuilder.DropColumn(
                name: "preliminary_detention_days",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "preliminary_detention_months",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "preliminary_detention_weeks",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "preliminary_detention_years",
                table: "case_person_sentence_punishment");
        }
    }
}
