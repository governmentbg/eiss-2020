using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentRequestTypeIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "document_request_type_id",
                table: "document",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_document_request_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    label = table.Column<string>(type: "text", nullable: true),
                    document_group_id = table.Column<int>(type: "integer", nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    case_group_id = table.Column<int>(type: "integer", nullable: false),
                    case_type_id = table.Column<int>(type: "integer", nullable: false),
                    case_code_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_document_request_type", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_document_request_type_id",
                table: "document",
                column: "document_request_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_nom_document_request_type_document_request_type_id",
                table: "document",
                column: "document_request_type_id",
                principalTable: "nom_document_request_type",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_nom_document_request_type_document_request_type_id",
                table: "document");

            migrationBuilder.DropTable(
                name: "nom_document_request_type");

            migrationBuilder.DropIndex(
                name: "IX_document_document_request_type_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "document_request_type_id",
                table: "document");
        }
    }
}
