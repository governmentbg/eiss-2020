using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CoordinationDepersonalizationData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "depersonalize_end_date",
                table: "case_session_act_coordination",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "depersonalize_user_id",
                table: "case_session_act_coordination",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_coordination_depersonalize_user_id",
                table: "case_session_act_coordination",
                column: "depersonalize_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_coordination_identity_users_depersonalize_~",
                table: "case_session_act_coordination",
                column: "depersonalize_user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_coordination_identity_users_depersonalize_~",
                table: "case_session_act_coordination");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_coordination_depersonalize_user_id",
                table: "case_session_act_coordination");

            migrationBuilder.DropColumn(
                name: "depersonalize_end_date",
                table: "case_session_act_coordination");

            migrationBuilder.DropColumn(
                name: "depersonalize_user_id",
                table: "case_session_act_coordination");
        }
    }
}
