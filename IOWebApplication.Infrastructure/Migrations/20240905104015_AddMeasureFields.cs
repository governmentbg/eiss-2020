using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasureFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "measure_kind_id",
                table: "case_person_measures",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "measure_quantity",
                table: "case_person_measures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "measure_unit",
                table: "case_person_measures",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "measure_kind_id",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "measure_quantity",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "measure_unit",
                table: "case_person_measures");
        }
    }
}
