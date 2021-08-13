using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddTableCaseLoadElementTypeStop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_case_load_element_type_stop",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    case_load_element_type_id = table.Column<int>(nullable: false),
                    case_load_element_type_stop_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_case_load_element_type_stop", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_case_load_element_type_stop_nom_case_load_element_type_~",
                        column: x => x.case_load_element_type_id,
                        principalTable: "nom_case_load_element_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_case_load_element_type_stop_nom_case_load_element_type~1",
                        column: x => x.case_load_element_type_stop_id,
                        principalTable: "nom_case_load_element_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_stop_case_load_element_type_id",
                table: "nom_case_load_element_type_stop",
                column: "case_load_element_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_stop_case_load_element_type_stop~",
                table: "nom_case_load_element_type_stop",
                column: "case_load_element_type_stop_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_case_load_element_type_stop");
        }
    }
}
