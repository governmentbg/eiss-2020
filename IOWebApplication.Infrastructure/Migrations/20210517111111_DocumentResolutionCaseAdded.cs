using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentResolutionCaseAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_resolution_case",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    document_resolution_id = table.Column<long>(nullable: false),
                    case_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_resolution_case", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_resolution_case_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_resolution_case_document_resolution_document_resol~",
                        column: x => x.document_resolution_id,
                        principalTable: "document_resolution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_resolution_case_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_case_case_id",
                table: "document_resolution_case",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_case_document_resolution_id",
                table: "document_resolution_case",
                column: "document_resolution_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_case_user_id",
                table: "document_resolution_case",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_resolution_case");
        }
    }
}
