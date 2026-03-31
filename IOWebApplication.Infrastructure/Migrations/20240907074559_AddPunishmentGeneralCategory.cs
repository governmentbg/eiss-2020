using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPunishmentGeneralCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "punishment_general_category",
                table: "case_person_sentence_punishment",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_punishment_general_category",
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
                    table.PrimaryKey("PK_nom_punishment_general_category", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_punishment_punishment_general_category",
                table: "case_person_sentence_punishment",
                column: "punishment_general_category");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_punishment_nom_punishment_general_cate~",
                table: "case_person_sentence_punishment",
                column: "punishment_general_category",
                principalTable: "nom_punishment_general_category",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_punishment_nom_punishment_general_cate~",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropTable(
                name: "nom_punishment_general_category");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_punishment_punishment_general_category",
                table: "case_person_sentence_punishment");

            migrationBuilder.DropColumn(
                name: "punishment_general_category",
                table: "case_person_sentence_punishment");
        }
    }
}
