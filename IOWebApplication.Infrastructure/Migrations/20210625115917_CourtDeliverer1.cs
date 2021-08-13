using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CourtDeliverer1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_court_deliverer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_court_deliverer",
                columns: table => new
                {
                    court_id = table.Column<string>(nullable: false),
                    ekatte = table.Column<string>(nullable: false),
                    deiverer_court_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_court_deliverer", x => new { x.court_id, x.ekatte });
                });
        }
    }
}
