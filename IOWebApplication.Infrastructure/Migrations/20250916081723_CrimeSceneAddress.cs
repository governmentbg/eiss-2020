using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrimeSceneAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "crime_scene_appartment",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "crime_scene_building",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "crime_scene_entrance",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "crime_scene_floor",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "crime_scene_localization",
                table: "case_crimes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "crime_scene_number",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "crime_scene_settlement_abroad",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "crime_scene_street_name",
                table: "case_crimes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "crime_scene_appartment",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_building",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_entrance",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_floor",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_localization",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_number",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_settlement_abroad",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_street_name",
                table: "case_crimes");
        }
    }
}
