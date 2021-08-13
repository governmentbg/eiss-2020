using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CourtDeliverer2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_court_deliverer",
                columns: table => new
                {
                    court_id = table.Column<int>(nullable: false),
                    ekatte = table.Column<string>(nullable: false),
                    deiverer_court_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_court_deliverer", x => new { x.court_id, x.ekatte });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_court_deliverer");
        }
    }
}
