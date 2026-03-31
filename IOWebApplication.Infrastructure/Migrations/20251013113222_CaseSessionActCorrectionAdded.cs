using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseSessionActCorrectionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_case_session_act_corrected_act_id",
                table: "case_session_act");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_corrected_act_id",
                table: "case_session_act");

            migrationBuilder.CreateTable(
                name: "case_session_act_correction",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_session_act_id = table.Column<int>(type: "integer", nullable: false),
                    corrected_act_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_session_act_correction", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_session_act_correction_case_session_act_case_session_a~",
                        column: x => x.case_session_act_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_correction_case_session_act_corrected_act_~",
                        column: x => x.corrected_act_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_correction_case_session_act_id",
                table: "case_session_act_correction",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_correction_corrected_act_id",
                table: "case_session_act_correction",
                column: "corrected_act_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_session_act_correction");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_corrected_act_id",
                table: "case_session_act",
                column: "corrected_act_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_case_session_act_corrected_act_id",
                table: "case_session_act",
                column: "corrected_act_id",
                principalTable: "case_session_act",
                principalColumn: "id");
        }
    }
}
