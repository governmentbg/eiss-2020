using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class SismaIndexRecapContains : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sisma_index_contains",
                table: "nom_sisma_index_recap");

            migrationBuilder.CreateTable(
                name: "nom_sisma_index_recap_contains",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    sisma_index_recap_id = table.Column<int>(nullable: false),
                    sisma_index_contains = table.Column<string>(nullable: true),
                    sign = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_sisma_index_recap_contains", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_sisma_index_recap_contains_nom_sisma_index_recap_sisma_~",
                        column: x => x.sisma_index_recap_id,
                        principalTable: "nom_sisma_index_recap",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_sisma_index_recap_contains_sisma_index_recap_id",
                table: "nom_sisma_index_recap_contains",
                column: "sisma_index_recap_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_sisma_index_recap_contains");

            migrationBuilder.AddColumn<string>(
                name: "sisma_index_contains",
                table: "nom_sisma_index_recap",
                nullable: true);
        }
    }
}
