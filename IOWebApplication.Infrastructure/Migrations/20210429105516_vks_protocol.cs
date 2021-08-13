using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class vks_protocol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_signed",
                table: "vks_selection_protocol",
                newName: "user_signed_id");

            migrationBuilder.RenameColumn(
                name: "user_generated",
                table: "vks_selection_protocol",
                newName: "user_generated_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_protocol_user_generated_id",
                table: "vks_selection_protocol",
                column: "user_generated_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_protocol_user_signed_id",
                table: "vks_selection_protocol",
                column: "user_signed_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vks_selection_protocol_identity_users_user_generated_id",
                table: "vks_selection_protocol",
                column: "user_generated_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vks_selection_protocol_identity_users_user_signed_id",
                table: "vks_selection_protocol",
                column: "user_signed_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vks_selection_protocol_identity_users_user_generated_id",
                table: "vks_selection_protocol");

            migrationBuilder.DropForeignKey(
                name: "FK_vks_selection_protocol_identity_users_user_signed_id",
                table: "vks_selection_protocol");

            migrationBuilder.DropIndex(
                name: "IX_vks_selection_protocol_user_generated_id",
                table: "vks_selection_protocol");

            migrationBuilder.DropIndex(
                name: "IX_vks_selection_protocol_user_signed_id",
                table: "vks_selection_protocol");

            migrationBuilder.RenameColumn(
                name: "user_signed_id",
                table: "vks_selection_protocol",
                newName: "user_signed");

            migrationBuilder.RenameColumn(
                name: "user_generated_id",
                table: "vks_selection_protocol",
                newName: "user_generated");
        }
    }
}
