using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class LawUnitSubstitutionAlter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "identity_users",
                nullable: false,
                oldClrType: typeof(bool),
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_court_lawunit_substitution",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_transfered_dw",
                table: "common_court_lawunit_substitution",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_wrt",
                table: "common_court_lawunit_substitution",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "common_court_lawunit_substitution",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_substitution_user_id",
                table: "common_court_lawunit_substitution",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_court_lawunit_substitution_identity_users_user_id",
                table: "common_court_lawunit_substitution",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_court_lawunit_substitution_identity_users_user_id",
                table: "common_court_lawunit_substitution");

            migrationBuilder.DropIndex(
                name: "IX_common_court_lawunit_substitution_user_id",
                table: "common_court_lawunit_substitution");

            migrationBuilder.DropColumn(
                name: "date_transfered_dw",
                table: "common_court_lawunit_substitution");

            migrationBuilder.DropColumn(
                name: "date_wrt",
                table: "common_court_lawunit_substitution");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "common_court_lawunit_substitution");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "identity_users",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool));

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_court_lawunit_substitution",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
