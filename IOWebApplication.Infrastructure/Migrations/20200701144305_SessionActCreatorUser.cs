using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class SessionActCreatorUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "act_creator_user_id",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motive_creator_user_id",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "act_creator_user_id",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motive_creator_user_id",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_act_creator_user_id",
                table: "case_session_act",
                column: "act_creator_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_motive_creator_user_id",
                table: "case_session_act",
                column: "motive_creator_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_identity_users_act_creator_user_id",
                table: "case_session_act",
                column: "act_creator_user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_identity_users_motive_creator_user_id",
                table: "case_session_act",
                column: "motive_creator_user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_identity_users_act_creator_user_id",
                table: "case_session_act");

            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_identity_users_motive_creator_user_id",
                table: "case_session_act");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_act_creator_user_id",
                table: "case_session_act");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_motive_creator_user_id",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "act_creator_user_id",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "motive_creator_user_id",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "act_creator_user_id",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "motive_creator_user_id",
                table: "case_session_act");
        }
    }
}
