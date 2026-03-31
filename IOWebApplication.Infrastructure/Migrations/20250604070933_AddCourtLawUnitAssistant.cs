using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourtLawUnitAssistant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_court_lawunit_assistant",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_law_unit_id = table.Column<int>(type: "integer", nullable: false),
                    lawunit_id = table.Column<int>(type: "integer", nullable: false),
                    judge_role_id = table.Column<int>(type: "integer", nullable: false),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_court_lawunit_assistant", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_assistant_common_court_lawunit_court_l~",
                        column: x => x.court_law_unit_id,
                        principalTable: "common_court_lawunit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_assistant_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_assistant_nom_judge_role_judge_role_id",
                        column: x => x.judge_role_id,
                        principalTable: "nom_judge_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_assistant_court_law_unit_id",
                table: "common_court_lawunit_assistant",
                column: "court_law_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_assistant_judge_role_id",
                table: "common_court_lawunit_assistant",
                column: "judge_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_assistant_lawunit_id",
                table: "common_court_lawunit_assistant",
                column: "lawunit_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_court_lawunit_assistant");
        }
    }
}
