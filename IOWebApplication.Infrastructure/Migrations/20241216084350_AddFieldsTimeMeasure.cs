using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsTimeMeasure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "measure_days",
                table: "case_person_measures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "measure_months",
                table: "case_person_measures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "measure_weeks",
                table: "case_person_measures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "measure_years",
                table: "case_person_measures",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "measure_days",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "measure_months",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "measure_weeks",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "measure_years",
                table: "case_person_measures");
        }
    }
}
