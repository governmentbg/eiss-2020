using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCasePersonInheritance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "case_person_inheritance",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "case_person_inheritance",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "case_person_inheritance",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_person_inheritance_user_expired_id",
                table: "case_person_inheritance",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_inheritance_identity_users_user_expired_id",
                table: "case_person_inheritance",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_inheritance_identity_users_user_expired_id",
                table: "case_person_inheritance");

            migrationBuilder.DropIndex(
                name: "IX_case_person_inheritance_user_expired_id",
                table: "case_person_inheritance");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "case_person_inheritance");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "case_person_inheritance");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "case_person_inheritance");
        }
    }
}
