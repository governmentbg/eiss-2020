using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class WorkingDay_UserDateWRT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_transfered_dw",
                table: "common_working_day",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_wrt",
                table: "common_working_day",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "common_working_day",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_working_day_user_id",
                table: "common_working_day",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_working_day_identity_users_user_id",
                table: "common_working_day",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_working_day_identity_users_user_id",
                table: "common_working_day");

            migrationBuilder.DropIndex(
                name: "IX_common_working_day_user_id",
                table: "common_working_day");

            migrationBuilder.DropColumn(
                name: "date_transfered_dw",
                table: "common_working_day");

            migrationBuilder.DropColumn(
                name: "date_wrt",
                table: "common_working_day");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "common_working_day");
        }
    }
}
