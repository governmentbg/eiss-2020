using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrimeSceneCityEispp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "crime_scene_city_eispp_id",
                table: "case_crimes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_crimes_crime_scene_city_eispp_id",
                table: "case_crimes",
                column: "crime_scene_city_eispp_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_crimes_eispp_ektte_code_crime_scene_city_eispp_id",
                table: "case_crimes",
                column: "crime_scene_city_eispp_id",
                principalTable: "eispp_ektte_code",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_crimes_eispp_ektte_code_crime_scene_city_eispp_id",
                table: "case_crimes");

            migrationBuilder.DropIndex(
                name: "IX_case_crimes_crime_scene_city_eispp_id",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_city_eispp_id",
                table: "case_crimes");
        }
    }
}
