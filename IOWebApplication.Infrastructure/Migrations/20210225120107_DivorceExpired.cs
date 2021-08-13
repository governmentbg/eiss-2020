using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DivorceExpired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "case_session_act_divorce",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "case_session_act_divorce",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_divorce_user_expired_id",
                table: "case_session_act_divorce",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_divorce_identity_users_user_expired_id",
                table: "case_session_act_divorce",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_divorce_identity_users_user_expired_id",
                table: "case_session_act_divorce");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_divorce_user_expired_id",
                table: "case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "case_session_act_divorce");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "case_session_act_divorce");
        }
    }
}
