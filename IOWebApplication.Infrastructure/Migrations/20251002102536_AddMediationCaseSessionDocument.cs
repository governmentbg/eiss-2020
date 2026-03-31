using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediationCaseSessionDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mediation_case_session_document",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на запис")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на съд"),
                    case_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на дело"),
                    mediation_case_session_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на среща"),
                    date_upload = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на качване"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Забележка"),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на анулиране"),
                    user_expired_id = table.Column<string>(type: "text", nullable: true, comment: "Идентификатор на потребител, който е анулирал записа"),
                    description_expired = table.Column<string>(type: "text", nullable: true, comment: "Причина за анулиране"),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_session_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_document_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_document_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_document_identity_users_user_expired~",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_document_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_document_mediation_case_session_medi~",
                        column: x => x.mediation_case_session_id,
                        principalTable: "mediation_case_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Документи към среща за медиация");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_document_case_id",
                table: "mediation_case_session_document",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_document_court_id",
                table: "mediation_case_session_document",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_document_mediation_case_session_id",
                table: "mediation_case_session_document",
                column: "mediation_case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_document_user_expired_id",
                table: "mediation_case_session_document",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_document_user_id",
                table: "mediation_case_session_document",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mediation_case_session_document");
        }
    }
}
