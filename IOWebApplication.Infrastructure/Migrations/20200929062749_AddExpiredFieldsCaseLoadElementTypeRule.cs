using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddExpiredFieldsCaseLoadElementTypeRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "nom_case_load_element_type_rule",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "nom_case_load_element_type_rule",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "nom_case_load_element_type_rule",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_rule_user_expired_id",
                table: "nom_case_load_element_type_rule",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_rule_identity_users_user_expired~",
                table: "nom_case_load_element_type_rule",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_rule_identity_users_user_expired~",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropIndex(
                name: "IX_nom_case_load_element_type_rule_user_expired_id",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "nom_case_load_element_type_rule");
        }
    }
}
