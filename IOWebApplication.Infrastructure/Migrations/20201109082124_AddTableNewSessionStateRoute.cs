using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddTableNewSessionStateRoute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_session_state_route",
                columns: table => new
                {
                    session_state_from_id = table.Column<int>(nullable: false),
                    session_state_to_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_session_state_route", x => new { x.session_state_from_id, x.session_state_to_id });
                    table.ForeignKey(
                        name: "FK_nom_session_state_route_nom_session_state_session_state_to_~",
                        column: x => x.session_state_to_id,
                        principalTable: "nom_session_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_session_state_route_session_state_to_id",
                table: "nom_session_state_route",
                column: "session_state_to_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_session_state_route");
        }
    }
}
