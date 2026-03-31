using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExcelSismaMapping1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_excel_sisma_mapping",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_type_id = table.Column<int>(nullable: false),
                    sheet_index = table.Column<int>(nullable: false),
                    row_index = table.Column<int>(nullable: false),
                    col_index = table.Column<int>(nullable: false),
                    sisma_index = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_excel_sisma_mapping", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_excel_sisma_mapping_nom_court_type_court_type_id",
                        column: x => x.court_type_id,
                        principalTable: "nom_court_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_excel_sisma_mapping_court_type_id",
                table: "nom_excel_sisma_mapping",
                column: "court_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_excel_sisma_mapping");
        }
    }
}
