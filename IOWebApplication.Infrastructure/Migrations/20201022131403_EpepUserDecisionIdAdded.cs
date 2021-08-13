using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EpepUserDecisionIdAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "document_decision_id",
                table: "epep_user",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "common_mongo_file",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "common_mongo_file",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "common_mongo_file",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_mongo_file_user_expired_id",
                table: "common_mongo_file",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_mongo_file_identity_users_user_expired_id",
                table: "common_mongo_file",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_mongo_file_identity_users_user_expired_id",
                table: "common_mongo_file");

            migrationBuilder.DropIndex(
                name: "IX_common_mongo_file_user_expired_id",
                table: "common_mongo_file");

            migrationBuilder.DropColumn(
                name: "document_decision_id",
                table: "epep_user");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "common_mongo_file");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "common_mongo_file");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "common_mongo_file");
        }
    }
}
