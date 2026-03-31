using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MeditationNotificationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkDirectionId",
                table: "mediation_notification",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "link_direction_second_id",
                table: "mediation_notification",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkDirectionId",
                table: "mediation_notification");

            migrationBuilder.DropColumn(
                name: "link_direction_second_id",
                table: "mediation_notification");
        }
    }
}
