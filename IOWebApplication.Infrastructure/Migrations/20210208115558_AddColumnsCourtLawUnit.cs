using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCourtLawUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "common_court_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "common_court_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "common_court_lawunit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_user_expired_id",
                table: "common_court_lawunit",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_court_lawunit_identity_users_user_expired_id",
                table: "common_court_lawunit",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_court_lawunit_identity_users_user_expired_id",
                table: "common_court_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_common_court_lawunit_user_expired_id",
                table: "common_court_lawunit");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "common_court_lawunit");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "common_court_lawunit");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "common_court_lawunit");
        }
    }
}
