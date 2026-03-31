using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediationCaseSessionLinkCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mediation_case_session_link_case",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на запис")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на среща за медиация"),
                    link_case_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на свързано дело")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_session_link_case", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_link_case_case_link_case_id",
                        column: x => x.link_case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_link_case_mediation_case_session_cou~",
                        column: x => x.court_id,
                        principalTable: "mediation_case_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Свързани дела към среща за медиация");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_link_case_court_id",
                table: "mediation_case_session_link_case",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_link_case_link_case_id",
                table: "mediation_case_session_link_case",
                column: "link_case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mediation_case_session_link_case");
        }
    }
}
