using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFKCaseNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_case_notification_to_court_id",
                table: "case_notification",
                column: "to_court_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_notification_common_court_to_court_id",
                table: "case_notification",
                column: "to_court_id",
                principalTable: "common_court",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_notification_common_court_to_court_id",
                table: "case_notification");

            migrationBuilder.DropIndex(
                name: "IX_case_notification_to_court_id",
                table: "case_notification");
        }
    }
}
