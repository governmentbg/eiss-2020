using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentTemplateExpired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "document_template",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "document_template",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "document_template",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_template_user_expired_id",
                table: "document_template",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_template_identity_users_user_expired_id",
                table: "document_template",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_template_identity_users_user_expired_id",
                table: "document_template");

            migrationBuilder.DropIndex(
                name: "IX_document_template_user_expired_id",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "document_template");
        }
    }
}
