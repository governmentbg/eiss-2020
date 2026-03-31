using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditCasePersonSentenceBulletin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "birth_day_place_city_id",
                table: "case_person_sentence_bulletin",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "birth_day_place_city_text",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "birth_day_place_country_id",
                table: "case_person_sentence_bulletin",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "father_name_latin",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "issuing_country_id",
                table: "case_person_sentence_bulletin",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mother_name_latin",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nationality_country_one_id",
                table: "case_person_sentence_bulletin",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nationality_country_two_id",
                table: "case_person_sentence_bulletin",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nick_name",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "number_afis",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_birth_day_place_city_id",
                table: "case_person_sentence_bulletin",
                column: "birth_day_place_city_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_birth_day_place_country_id",
                table: "case_person_sentence_bulletin",
                column: "birth_day_place_country_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_issuing_country_id",
                table: "case_person_sentence_bulletin",
                column: "issuing_country_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_nationality_country_one_id",
                table: "case_person_sentence_bulletin",
                column: "nationality_country_one_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_nationality_country_two_id",
                table: "case_person_sentence_bulletin",
                column: "nationality_country_two_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_birth_day_place_~",
                table: "case_person_sentence_bulletin",
                column: "birth_day_place_country_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_issuing_country_~",
                table: "case_person_sentence_bulletin",
                column: "issuing_country_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_nationality_coun~",
                table: "case_person_sentence_bulletin",
                column: "nationality_country_one_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_nationality_cou~1",
                table: "case_person_sentence_bulletin",
                column: "nationality_country_two_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_ekatte_birth_day_place_cit~",
                table: "case_person_sentence_bulletin",
                column: "birth_day_place_city_id",
                principalTable: "ek_ekatte",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_birth_day_place_~",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_issuing_country_~",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_nationality_coun~",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_countries_nationality_cou~1",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_ek_ekatte_birth_day_place_cit~",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_bulletin_birth_day_place_city_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_bulletin_birth_day_place_country_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_bulletin_issuing_country_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_bulletin_nationality_country_one_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_bulletin_nationality_country_two_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "birth_day_place_city_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "birth_day_place_city_text",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "birth_day_place_country_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "father_name_latin",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "issuing_country_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "mother_name_latin",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "nationality_country_one_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "nationality_country_two_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "nick_name",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "number_afis",
                table: "case_person_sentence_bulletin");
        }
    }
}
