using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseLawUnitTaskChangeAlter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_task_change_new_task_user_id",
                table: "case_lawunit_task_change",
                column: "new_task_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_task_change_identity_users_new_task_user_id",
                table: "case_lawunit_task_change",
                column: "new_task_user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_task_change_identity_users_new_task_user_id",
                table: "case_lawunit_task_change");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_task_change_new_task_user_id",
                table: "case_lawunit_task_change");
        }
    }
}
