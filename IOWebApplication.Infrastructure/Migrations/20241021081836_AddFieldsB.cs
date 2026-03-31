using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "birth_day_place_description_cir",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "birth_day_place_description_lat",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "other_uic",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "other_uic_issuing_country_id",
                table: "case_person_sentence_bulletin",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "other_uic_place_city_id",
                table: "case_person_sentence_bulletin",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birth_day_place_description_cir",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "birth_day_place_description_lat",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "other_uic",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "other_uic_issuing_country_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "other_uic_place_city_id",
                table: "case_person_sentence_bulletin");
        }
    }
}
