using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddExpiredActComplainSessionDoc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "case_session_doc",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "case_session_doc",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "case_session_doc",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "case_session_act_complain",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "case_session_act_complain",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_doc_user_expired_id",
                table: "case_session_doc",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_complain_user_expired_id",
                table: "case_session_act_complain",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_complain_identity_users_user_expired_id",
                table: "case_session_act_complain",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_doc_identity_users_user_expired_id",
                table: "case_session_doc",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_complain_identity_users_user_expired_id",
                table: "case_session_act_complain");

            migrationBuilder.DropForeignKey(
                name: "FK_case_session_doc_identity_users_user_expired_id",
                table: "case_session_doc");

            migrationBuilder.DropIndex(
                name: "IX_case_session_doc_user_expired_id",
                table: "case_session_doc");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_complain_user_expired_id",
                table: "case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "case_session_doc");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "case_session_doc");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "case_session_doc");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "case_session_act_complain");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "case_session_act_complain");
        }
    }
}
