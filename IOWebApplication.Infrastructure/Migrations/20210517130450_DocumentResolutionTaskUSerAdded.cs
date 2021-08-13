using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentResolutionTaskUSerAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "task_user_id",
                table: "document_resolution",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_task_user_id",
                table: "document_resolution",
                column: "task_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_resolution_identity_users_task_user_id",
                table: "document_resolution",
                column: "task_user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_resolution_identity_users_task_user_id",
                table: "document_resolution");

            migrationBuilder.DropIndex(
                name: "IX_document_resolution_task_user_id",
                table: "document_resolution");

            migrationBuilder.DropColumn(
                name: "task_user_id",
                table: "document_resolution");
        }
    }
}
