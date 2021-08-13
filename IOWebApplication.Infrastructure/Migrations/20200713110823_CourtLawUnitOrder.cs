using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CourtLawUnitOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "process_type",
                table: "nom_act_kind",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "common_court_lawunit_order",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_id = table.Column<int>(nullable: false),
                    lawunit_id = table.Column<int>(nullable: false),
                    order_number = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_court_lawunit_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_order_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_order_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_order_court_id",
                table: "common_court_lawunit_order",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_order_lawunit_id",
                table: "common_court_lawunit_order",
                column: "lawunit_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_court_lawunit_order");

            migrationBuilder.DropColumn(
                name: "process_type",
                table: "nom_act_kind");
        }
    }
}
