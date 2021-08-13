using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CasePersonLink_UserDateWRT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_transfered_dw",
                table: "case_person_link",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_wrt",
                table: "case_person_link",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "case_person_link",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_person_link_user_id",
                table: "case_person_link",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_link_identity_users_user_id",
                table: "case_person_link",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_link_identity_users_user_id",
                table: "case_person_link");

            migrationBuilder.DropIndex(
                name: "IX_case_person_link_user_id",
                table: "case_person_link");

            migrationBuilder.DropColumn(
                name: "date_transfered_dw",
                table: "case_person_link");

            migrationBuilder.DropColumn(
                name: "date_wrt",
                table: "case_person_link");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "case_person_link");
        }
    }
}
