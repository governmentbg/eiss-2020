using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentRequestInfoAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_request_info",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<long>(type: "bigint", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: true),
                    money_fee_type_id = table.Column<int>(type: "integer", nullable: true),
                    base_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    base_amount_bgn = table.Column<decimal>(type: "numeric", nullable: true),
                    tax_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    tax_amount_bgn = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_request_info", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_request_info_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_document_request_info_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_document_request_info_nom_money_fee_type_money_fee_type_id",
                        column: x => x.money_fee_type_id,
                        principalTable: "nom_money_fee_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_request_info_case_id",
                table: "document_request_info",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_request_info_document_id",
                table: "document_request_info",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_request_info_money_fee_type_id",
                table: "document_request_info",
                column: "money_fee_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_request_info");
        }
    }
}
