using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CourtApiKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_court_api_key",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_id = table.Column<int>(nullable: false),
                    integration_type_id = table.Column<int>(nullable: false),
                    key = table.Column<string>(nullable: true),
                    secret = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_court_api_key", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_court_api_key_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_api_key_nom_integration_type_integration_type_~",
                        column: x => x.integration_type_id,
                        principalTable: "nom_integration_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_court_api_key_court_id",
                table: "common_court_api_key",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_api_key_integration_type_id",
                table: "common_court_api_key",
                column: "integration_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_court_api_key");
        }
    }
}
