using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCasePersonSentencePunishmentCrime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SentenceTypeId",
                table: "case_person_sentence_punishment_crime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sentence_days",
                table: "case_person_sentence_punishment_crime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "sentence_money",
                table: "case_person_sentence_punishment_crime",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "sentence_months",
                table: "case_person_sentence_punishment_crime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sentence_weeks",
                table: "case_person_sentence_punishment_crime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sentence_years",
                table: "case_person_sentence_punishment_crime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_punishment_crime_SentenceTypeId",
                table: "case_person_sentence_punishment_crime",
                column: "SentenceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_punishment_crime_nom_sentence_type_Sen~",
                table: "case_person_sentence_punishment_crime",
                column: "SentenceTypeId",
                principalTable: "nom_sentence_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_punishment_crime_nom_sentence_type_Sen~",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_punishment_crime_SentenceTypeId",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropColumn(
                name: "SentenceTypeId",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropColumn(
                name: "sentence_days",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropColumn(
                name: "sentence_money",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropColumn(
                name: "sentence_months",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropColumn(
                name: "sentence_weeks",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropColumn(
                name: "sentence_years",
                table: "case_person_sentence_punishment_crime");
        }
    }
}
