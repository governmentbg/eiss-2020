using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ElectionAltered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_election_log_election_group_ElectionGroupId",
                table: "election_log");

            migrationBuilder.DropForeignKey(
                name: "FK_election_log_identity_users_UserId",
                table: "election_log");

            migrationBuilder.RenameColumn(
                name: "Operation",
                table: "election_log",
                newName: "operation");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "election_log",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "election_log",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PersonGid",
                table: "election_log",
                newName: "person_gid");

            migrationBuilder.RenameColumn(
                name: "LawunitId",
                table: "election_log",
                newName: "lawunit_id");

            migrationBuilder.RenameColumn(
                name: "ElectionGroupId",
                table: "election_log",
                newName: "election_group_id");

            migrationBuilder.RenameColumn(
                name: "DateWrt",
                table: "election_log",
                newName: "date_wrt");

            migrationBuilder.RenameIndex(
                name: "IX_election_log_UserId",
                table: "election_log",
                newName: "IX_election_log_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_election_log_ElectionGroupId",
                table: "election_log",
                newName: "IX_election_log_election_group_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_election",
                table: "election_protocol",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_election_id",
                table: "election_protocol",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_decalration",
                table: "election_person",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "has_decalration",
                table: "election_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "election_protocol_id",
                table: "election_log",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_user_election_id",
                table: "election_protocol",
                column: "user_election_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_log_election_protocol_id",
                table: "election_log",
                column: "election_protocol_id");

            migrationBuilder.AddForeignKey(
                name: "FK_election_log_election_group_election_group_id",
                table: "election_log",
                column: "election_group_id",
                principalTable: "election_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_election_log_election_protocol_election_protocol_id",
                table: "election_log",
                column: "election_protocol_id",
                principalTable: "election_protocol",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_election_log_identity_users_user_id",
                table: "election_log",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_election_protocol_identity_users_user_election_id",
                table: "election_protocol",
                column: "user_election_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_election_log_election_group_election_group_id",
                table: "election_log");

            migrationBuilder.DropForeignKey(
                name: "FK_election_log_election_protocol_election_protocol_id",
                table: "election_log");

            migrationBuilder.DropForeignKey(
                name: "FK_election_log_identity_users_user_id",
                table: "election_log");

            migrationBuilder.DropForeignKey(
                name: "FK_election_protocol_identity_users_user_election_id",
                table: "election_protocol");

            migrationBuilder.DropIndex(
                name: "IX_election_protocol_user_election_id",
                table: "election_protocol");

            migrationBuilder.DropIndex(
                name: "IX_election_log_election_protocol_id",
                table: "election_log");

            migrationBuilder.DropColumn(
                name: "date_election",
                table: "election_protocol");

            migrationBuilder.DropColumn(
                name: "user_election_id",
                table: "election_protocol");

            migrationBuilder.DropColumn(
                name: "date_decalration",
                table: "election_person");

            migrationBuilder.DropColumn(
                name: "has_decalration",
                table: "election_person");

            migrationBuilder.DropColumn(
                name: "election_protocol_id",
                table: "election_log");

            migrationBuilder.RenameColumn(
                name: "operation",
                table: "election_log",
                newName: "Operation");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "election_log",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "election_log",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "person_gid",
                table: "election_log",
                newName: "PersonGid");

            migrationBuilder.RenameColumn(
                name: "lawunit_id",
                table: "election_log",
                newName: "LawunitId");

            migrationBuilder.RenameColumn(
                name: "election_group_id",
                table: "election_log",
                newName: "ElectionGroupId");

            migrationBuilder.RenameColumn(
                name: "date_wrt",
                table: "election_log",
                newName: "DateWrt");

            migrationBuilder.RenameIndex(
                name: "IX_election_log_user_id",
                table: "election_log",
                newName: "IX_election_log_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_election_log_election_group_id",
                table: "election_log",
                newName: "IX_election_log_ElectionGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_election_log_election_group_ElectionGroupId",
                table: "election_log",
                column: "ElectionGroupId",
                principalTable: "election_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_election_log_identity_users_UserId",
                table: "election_log",
                column: "UserId",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
