using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BuletinExports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reg_date",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "reg_number",
                table: "case_person_sentence_bulletin");

            migrationBuilder.CreateTable(
                name: "case_person_sentence_bulletin_export",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_person_sentence_bulletin__id = table.Column<int>(type: "integer", nullable: false),
                    reg_number = table.Column<string>(type: "text", nullable: true),
                    reg_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reg_user_id = table.Column<string>(type: "text", nullable: true),
                    date_export = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_person_sentence_bulletin_export", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_person_sentence_bulletin_export_case_person_sentence_b~",
                        column: x => x.case_person_sentence_bulletin__id,
                        principalTable: "case_person_sentence_bulletin",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_person_sentence_bulletin_export_identity_users_reg_use~",
                        column: x => x.reg_user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_person_sentence_bulletin_export_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_export_case_person_sentence_b~",
                table: "case_person_sentence_bulletin_export",
                column: "case_person_sentence_bulletin__id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_export_reg_user_id",
                table: "case_person_sentence_bulletin_export",
                column: "reg_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_export_user_id",
                table: "case_person_sentence_bulletin_export",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_person_sentence_bulletin_export");

            migrationBuilder.AddColumn<DateTime>(
                name: "reg_date",
                table: "case_person_sentence_bulletin",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reg_number",
                table: "case_person_sentence_bulletin",
                type: "text",
                nullable: true);
        }
    }
}
