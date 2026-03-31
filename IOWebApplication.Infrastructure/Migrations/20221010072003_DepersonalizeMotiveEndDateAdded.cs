using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DepersonalizeMotiveEndDateAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "depersonalize_motive_end_date",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "depersonalize_motive_user_id",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "depersonalize_motive_end_date",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "depersonalize_motive_user_id",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_depersonalize_motive_user_id",
                table: "case_session_act",
                column: "depersonalize_motive_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_identity_users_depersonalize_motive_user_id",
                table: "case_session_act",
                column: "depersonalize_motive_user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_identity_users_depersonalize_motive_user_id",
                table: "case_session_act");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_depersonalize_motive_user_id",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "depersonalize_motive_end_date",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "depersonalize_motive_user_id",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "depersonalize_motive_end_date",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "depersonalize_motive_user_id",
                table: "case_session_act");
        }
    }
}
