using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentResolutionActCreator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "act_creator_user_id",
                table: "document_resolution",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_act_creator_user_id",
                table: "document_resolution",
                column: "act_creator_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_resolution_identity_users_act_creator_user_id",
                table: "document_resolution",
                column: "act_creator_user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_resolution_identity_users_act_creator_user_id",
                table: "document_resolution");

            migrationBuilder.DropIndex(
                name: "IX_document_resolution_act_creator_user_id",
                table: "document_resolution");

            migrationBuilder.DropColumn(
                name: "act_creator_user_id",
                table: "document_resolution");
        }
    }
}
