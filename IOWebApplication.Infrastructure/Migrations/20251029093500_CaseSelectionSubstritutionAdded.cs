using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseSelectionSubstritutionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_selection_substitution",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_selection_protokol_substitution_id = table.Column<int>(type: "integer", nullable: false),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    case_session_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_selection_substitution", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_selection_substitution_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_substitution_case_selection_protokol_substit~",
                        column: x => x.case_selection_protokol_substitution_id,
                        principalTable: "case_selection_protokol_substitution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_substitution_case_session_case_session_id",
                        column: x => x.case_session_id,
                        principalTable: "case_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_substitution_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_substitution_case_id",
                table: "case_selection_substitution",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_substitution_case_selection_protokol_substit~",
                table: "case_selection_substitution",
                column: "case_selection_protokol_substitution_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_substitution_case_session_id",
                table: "case_selection_substitution",
                column: "case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_substitution_user_id",
                table: "case_selection_substitution",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_selection_substitution");
        }
    }
}
