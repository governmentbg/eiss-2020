using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MediationNotificationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_person_l1_id",
                table: "mediation_notification",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_person_l2_id",
                table: "mediation_notification",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_person_l3_id",
                table: "mediation_notification",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "case_person_l1_id",
                table: "mediation_notification");

            migrationBuilder.DropColumn(
                name: "case_person_l2_id",
                table: "mediation_notification");

            migrationBuilder.DropColumn(
                name: "case_person_l3_id",
                table: "mediation_notification");
        }
    }
}
