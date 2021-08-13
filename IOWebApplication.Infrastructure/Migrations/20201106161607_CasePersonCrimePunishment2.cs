using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CasePersonCrimePunishment2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_case_p~",
                table: "case_person_crime_punishment");

            migrationBuilder.RenameColumn(
                name: "case_case_person_id",
                table: "case_person_crime_punishment",
                newName: "case_person_crime_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_crime_punishment_case_case_person_id",
                table: "case_person_crime_punishment",
                newName: "IX_case_person_crime_punishment_case_person_crime_id");

            migrationBuilder.AddColumn<int>(
                name: "case_person_sentence_punishment_crime_id",
                table: "case_person_crime_punishment",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_case_person_crime_punishment_case_person_sentence_punishmen~",
                table: "case_person_crime_punishment",
                column: "case_person_sentence_punishment_crime_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_person~",
                table: "case_person_crime_punishment",
                column: "case_person_crime_id",
                principalTable: "case_person_crimes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_crime_punishment_case_person_sentence_punishmen~",
                table: "case_person_crime_punishment",
                column: "case_person_sentence_punishment_crime_id",
                principalTable: "case_person_sentence_punishment_crime",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_person~",
                table: "case_person_crime_punishment");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_crime_punishment_case_person_sentence_punishmen~",
                table: "case_person_crime_punishment");

            migrationBuilder.DropIndex(
                name: "IX_case_person_crime_punishment_case_person_sentence_punishmen~",
                table: "case_person_crime_punishment");

            migrationBuilder.DropColumn(
                name: "case_person_sentence_punishment_crime_id",
                table: "case_person_crime_punishment");

            migrationBuilder.RenameColumn(
                name: "case_person_crime_id",
                table: "case_person_crime_punishment",
                newName: "case_case_person_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_crime_punishment_case_person_crime_id",
                table: "case_person_crime_punishment",
                newName: "IX_case_person_crime_punishment_case_case_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_case_p~",
                table: "case_person_crime_punishment",
                column: "case_case_person_id",
                principalTable: "case_person_crimes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
