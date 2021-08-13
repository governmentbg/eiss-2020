using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CasePersonCrimePunishment_CasePersonCrimeId_Nullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_person~",
                table: "case_person_crime_punishment");

            migrationBuilder.AlterColumn<int>(
                name: "case_person_crime_id",
                table: "case_person_crime_punishment",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_person~",
                table: "case_person_crime_punishment",
                column: "case_person_crime_id",
                principalTable: "case_person_crimes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_person~",
                table: "case_person_crime_punishment");

            migrationBuilder.AlterColumn<int>(
                name: "case_person_crime_id",
                table: "case_person_crime_punishment",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_crime_punishment_case_person_crimes_case_person~",
                table: "case_person_crime_punishment",
                column: "case_person_crime_id",
                principalTable: "case_person_crimes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
