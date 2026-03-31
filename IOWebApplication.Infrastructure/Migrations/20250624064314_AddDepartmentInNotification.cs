using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentInNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_court_department_id",
                table: "common_work_notification",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_work_notification_user_court_department_id",
                table: "common_work_notification",
                column: "user_court_department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_work_notification_common_court_department_user_court~",
                table: "common_work_notification",
                column: "user_court_department_id",
                principalTable: "common_court_department",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_work_notification_common_court_department_user_court~",
                table: "common_work_notification");

            migrationBuilder.DropIndex(
                name: "IX_common_work_notification_user_court_department_id",
                table: "common_work_notification");

            migrationBuilder.DropColumn(
                name: "user_court_department_id",
                table: "common_work_notification");
        }
    }
}
