using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DaysToPresenceSecretaryAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "notification_kind",
                table: "common_work_notification",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "secretary_user_id",
                table: "common_court_lawunit",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "days_to_presence",
                table: "common_court_group",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_secretary_user_id",
                table: "common_court_lawunit",
                column: "secretary_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_court_lawunit_identity_users_secretary_user_id",
                table: "common_court_lawunit",
                column: "secretary_user_id",
                principalTable: "identity_users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_court_lawunit_identity_users_secretary_user_id",
                table: "common_court_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_common_court_lawunit_secretary_user_id",
                table: "common_court_lawunit");

            migrationBuilder.DropColumn(
                name: "notification_kind",
                table: "common_work_notification");

            migrationBuilder.DropColumn(
                name: "secretary_user_id",
                table: "common_court_lawunit");

            migrationBuilder.DropColumn(
                name: "days_to_presence",
                table: "common_court_group");
        }
    }
}
