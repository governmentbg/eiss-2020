using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class RegixMapActualStateV3Object : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "for_display",
                table: "regix_map_actual_state",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type_field",
                table: "regix_map_actual_state",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "for_display",
                table: "regix_map_actual_state");

            migrationBuilder.DropColumn(
                name: "type_field",
                table: "regix_map_actual_state");
        }
    }
}
