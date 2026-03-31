using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseMigrationDocumentAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "document_declared_date",
                table: "document",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "document_process_type",
                table: "case_migration",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "case_migration_document",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_migration_id = table.Column<int>(type: "integer", nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_migration_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_migration_document_case_migration_case_migration_id",
                        column: x => x.case_migration_id,
                        principalTable: "case_migration",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_migration_document_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_migration_document_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_migration_document_case_migration_id",
                table: "case_migration_document",
                column: "case_migration_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_migration_document_user_expired_id",
                table: "case_migration_document",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_migration_document_user_id",
                table: "case_migration_document",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_migration_document");

            migrationBuilder.DropColumn(
                name: "document_declared_date",
                table: "document");

            migrationBuilder.DropColumn(
                name: "document_process_type",
                table: "case_migration");
        }
    }
}
