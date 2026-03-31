using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MeasureQuantityAddedAsInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "measure_quantity",
                table: "case_person_measures",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "measure_quantity",
                table: "case_person_measures");
        }
    }
}
