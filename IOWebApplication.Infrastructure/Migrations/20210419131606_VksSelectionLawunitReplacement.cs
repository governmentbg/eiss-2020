using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VksSelectionLawunitReplacement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lawunit_key",
                table: "vks_selection_month_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "selection_hash",
                table: "vks_selection_month",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_end",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_start",
                table: "vks_selection_lawunit",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lawunit_key",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "replaced_lawunit_id",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_lawunit_replaced_lawunit_id",
                table: "vks_selection_lawunit",
                column: "replaced_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_lawunit_user_expired_id",
                table: "vks_selection_lawunit",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vks_selection_lawunit_vks_selection_lawunit_replaced_lawuni~",
                table: "vks_selection_lawunit",
                column: "replaced_lawunit_id",
                principalTable: "vks_selection_lawunit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vks_selection_lawunit_identity_users_user_expired_id",
                table: "vks_selection_lawunit",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vks_selection_lawunit_vks_selection_lawunit_replaced_lawuni~",
                table: "vks_selection_lawunit");

            migrationBuilder.DropForeignKey(
                name: "FK_vks_selection_lawunit_identity_users_user_expired_id",
                table: "vks_selection_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_vks_selection_lawunit_replaced_lawunit_id",
                table: "vks_selection_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_vks_selection_lawunit_user_expired_id",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "lawunit_key",
                table: "vks_selection_month_lawunit");

            migrationBuilder.DropColumn(
                name: "selection_hash",
                table: "vks_selection_month");

            migrationBuilder.DropColumn(
                name: "date_end",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "date_start",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "lawunit_key",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "replaced_lawunit_id",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "vks_selection_lawunit");
        }
    }
}
