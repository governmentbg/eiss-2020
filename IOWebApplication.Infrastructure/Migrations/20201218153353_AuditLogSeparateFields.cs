using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AuditLogSeparateFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "base_object",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "client_ip",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_id",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "method",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "object_info",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "object_type",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "operation",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "request_url",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_ip",
                schema: "audit_log",
                table: "audit_log",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_court_id",
                schema: "audit_log",
                table: "audit_log",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_user_ip",
                schema: "audit_log",
                table: "audit_log",
                column: "user_ip");

            migrationBuilder.AddForeignKey(
                name: "FK_audit_log_common_court_court_id",
                schema: "audit_log",
                table: "audit_log",
                column: "court_id",
                principalTable: "common_court",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_audit_log_identity_users_user_ip",
                schema: "audit_log",
                table: "audit_log",
                column: "user_ip",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_audit_log_common_court_court_id",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropForeignKey(
                name: "FK_audit_log_identity_users_user_ip",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_court_id",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "IX_audit_log_user_ip",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "base_object",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "client_ip",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "court_id",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "full_name",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "method",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "object_info",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "object_type",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "operation",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "request_url",
                schema: "audit_log",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "user_ip",
                schema: "audit_log",
                table: "audit_log");
        }
    }
}
