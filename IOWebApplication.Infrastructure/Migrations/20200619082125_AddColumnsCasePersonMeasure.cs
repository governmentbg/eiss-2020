using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCasePersonMeasure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_measures_common_institution_measure_institution~",
                table: "case_person_measures");

            migrationBuilder.AlterColumn<string>(
                name: "measure_type",
                table: "case_person_measures",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "measure_institution_id",
                table: "case_person_measures",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "measure_court_id",
                table: "case_person_measures",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "parent_id",
                table: "case_person_measures",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_person_measures_measure_court_id",
                table: "case_person_measures",
                column: "measure_court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_measures_parent_id",
                table: "case_person_measures",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_measures_common_court_measure_court_id",
                table: "case_person_measures",
                column: "measure_court_id",
                principalTable: "common_court",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_measures_common_institution_measure_institution~",
                table: "case_person_measures",
                column: "measure_institution_id",
                principalTable: "common_institution",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_measures_case_person_measures_parent_id",
                table: "case_person_measures",
                column: "parent_id",
                principalTable: "case_person_measures",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_measures_common_court_measure_court_id",
                table: "case_person_measures");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_measures_common_institution_measure_institution~",
                table: "case_person_measures");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_measures_case_person_measures_parent_id",
                table: "case_person_measures");

            migrationBuilder.DropIndex(
                name: "IX_case_person_measures_measure_court_id",
                table: "case_person_measures");

            migrationBuilder.DropIndex(
                name: "IX_case_person_measures_parent_id",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "measure_court_id",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "case_person_measures");

            migrationBuilder.AlterColumn<string>(
                name: "measure_type",
                table: "case_person_measures",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "measure_institution_id",
                table: "case_person_measures",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_measures_common_institution_measure_institution~",
                table: "case_person_measures",
                column: "measure_institution_id",
                principalTable: "common_institution",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
