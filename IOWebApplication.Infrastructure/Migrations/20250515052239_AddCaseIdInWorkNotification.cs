using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseIdInWorkNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_id",
                table: "common_work_notification",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_work_notification_case_id",
                table: "common_work_notification",
                column: "case_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_work_notification_case_case_id",
                table: "common_work_notification",
                column: "case_id",
                principalTable: "case",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_work_notification_case_case_id",
                table: "common_work_notification");

            migrationBuilder.DropIndex(
                name: "IX_common_work_notification_case_id",
                table: "common_work_notification");

            migrationBuilder.DropColumn(
                name: "case_id",
                table: "common_work_notification");
        }
    }
}
