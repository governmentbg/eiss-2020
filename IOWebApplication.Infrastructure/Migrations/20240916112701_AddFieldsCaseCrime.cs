using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsCaseCrime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code_3",
                table: "ek_countries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code_number",
                table: "ek_countries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "ek_countries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "act_prior_probation",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "category_common_ceed_id",
                table: "case_crimes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "category_deed_id",
                table: "case_crimes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "crime_scene_city_id",
                table: "case_crimes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "crime_scene_country_id",
                table: "case_crimes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "crime_scene_text",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "form_guilt_id",
                table: "case_crimes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "has_prior_probation",
                table: "case_crimes",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "legal_qualification_text",
                table: "case_crimes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_category_common_deed",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_category_common_deed", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_category_deed",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_category_deed", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_crimes_category_common_ceed_id",
                table: "case_crimes",
                column: "category_common_ceed_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_crimes_category_deed_id",
                table: "case_crimes",
                column: "category_deed_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_crimes_crime_scene_city_id",
                table: "case_crimes",
                column: "crime_scene_city_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_crimes_crime_scene_country_id",
                table: "case_crimes",
                column: "crime_scene_country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_crimes_ek_countries_crime_scene_country_id",
                table: "case_crimes",
                column: "crime_scene_country_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_crimes_ek_ekatte_crime_scene_city_id",
                table: "case_crimes",
                column: "crime_scene_city_id",
                principalTable: "ek_ekatte",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_crimes_nom_category_common_deed_category_common_ceed_id",
                table: "case_crimes",
                column: "category_common_ceed_id",
                principalTable: "nom_category_common_deed",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_crimes_nom_category_deed_category_deed_id",
                table: "case_crimes",
                column: "category_deed_id",
                principalTable: "nom_category_deed",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_crimes_ek_countries_crime_scene_country_id",
                table: "case_crimes");

            migrationBuilder.DropForeignKey(
                name: "FK_case_crimes_ek_ekatte_crime_scene_city_id",
                table: "case_crimes");

            migrationBuilder.DropForeignKey(
                name: "FK_case_crimes_nom_category_common_deed_category_common_ceed_id",
                table: "case_crimes");

            migrationBuilder.DropForeignKey(
                name: "FK_case_crimes_nom_category_deed_category_deed_id",
                table: "case_crimes");

            migrationBuilder.DropTable(
                name: "nom_category_common_deed");

            migrationBuilder.DropTable(
                name: "nom_category_deed");

            migrationBuilder.DropIndex(
                name: "IX_case_crimes_category_common_ceed_id",
                table: "case_crimes");

            migrationBuilder.DropIndex(
                name: "IX_case_crimes_category_deed_id",
                table: "case_crimes");

            migrationBuilder.DropIndex(
                name: "IX_case_crimes_crime_scene_city_id",
                table: "case_crimes");

            migrationBuilder.DropIndex(
                name: "IX_case_crimes_crime_scene_country_id",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "code_3",
                table: "ek_countries");

            migrationBuilder.DropColumn(
                name: "code_number",
                table: "ek_countries");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "ek_countries");

            migrationBuilder.DropColumn(
                name: "act_prior_probation",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "category_common_ceed_id",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "category_deed_id",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_city_id",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_country_id",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "crime_scene_text",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "form_guilt_id",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "has_prior_probation",
                table: "case_crimes");

            migrationBuilder.DropColumn(
                name: "legal_qualification_text",
                table: "case_crimes");
        }
    }
}
