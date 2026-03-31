using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FastProcessNomenclatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "created_court_id",
                table: "document",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mongo_file_type_id",
                table: "common_mongo_file",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_fastprocess_417competency_base",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_fastprocess_417competency_base", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_fastprocess_claim_circumstance",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_fastprocess_claim_circumstance", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_fastprocess_expense_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_fastprocess_expense_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_fastprocess_money_claim_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_fastprocess_money_claim_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_mongo_file_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mongo_file_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_fastprocess_claim_circumstance_group",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FastProcessClaimCircumstanceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_fastprocess_claim_circumstance_group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nom_fastprocess_claim_circumstance_group_nom_fastprocess_cl~",
                        column: x => x.FastProcessClaimCircumstanceId,
                        principalTable: "nom_fastprocess_claim_circumstance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nom_mongo_file_type_grouping",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mongo_file_type_id = table.Column<int>(type: "integer", nullable: false),
                    type_group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mongo_file_type_grouping", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_mongo_file_type_grouping_nom_mongo_file_type_mongo_file~",
                        column: x => x.mongo_file_type_id,
                        principalTable: "nom_mongo_file_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_created_court_id",
                table: "document",
                column: "created_court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mongo_file_mongo_file_type_id",
                table: "common_mongo_file",
                column: "mongo_file_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_fastprocess_claim_circumstance_group_FastProcessClaimCi~",
                table: "nom_fastprocess_claim_circumstance_group",
                column: "FastProcessClaimCircumstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_nom_mongo_file_type_grouping_mongo_file_type_id",
                table: "nom_mongo_file_type_grouping",
                column: "mongo_file_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_mongo_file_nom_mongo_file_type_mongo_file_type_id",
                table: "common_mongo_file",
                column: "mongo_file_type_id",
                principalTable: "nom_mongo_file_type",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_common_court_created_court_id",
                table: "document",
                column: "created_court_id",
                principalTable: "common_court",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_mongo_file_nom_mongo_file_type_mongo_file_type_id",
                table: "common_mongo_file");

            migrationBuilder.DropForeignKey(
                name: "FK_document_common_court_created_court_id",
                table: "document");

            migrationBuilder.DropTable(
                name: "nom_fastprocess_417competency_base");

            migrationBuilder.DropTable(
                name: "nom_fastprocess_claim_circumstance_group");

            migrationBuilder.DropTable(
                name: "nom_fastprocess_expense_type");

            migrationBuilder.DropTable(
                name: "nom_fastprocess_money_claim_type");

            migrationBuilder.DropTable(
                name: "nom_mongo_file_type_grouping");

            migrationBuilder.DropTable(
                name: "nom_fastprocess_claim_circumstance");

            migrationBuilder.DropTable(
                name: "nom_mongo_file_type");

            migrationBuilder.DropIndex(
                name: "IX_document_created_court_id",
                table: "document");

            migrationBuilder.DropIndex(
                name: "IX_common_mongo_file_mongo_file_type_id",
                table: "common_mongo_file");

            migrationBuilder.DropColumn(
                name: "created_court_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "mongo_file_type_id",
                table: "common_mongo_file");
        }
    }
}
