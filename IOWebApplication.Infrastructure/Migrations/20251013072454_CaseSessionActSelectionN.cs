using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseSessionActSelectionN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_session_act_selection_protokol",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: false),
                    selected_lawunit_id = table.Column<int>(type: "integer", nullable: false),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    selection_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_final = table.Column<bool>(type: "boolean", nullable: false),
                    is_canceleling = table.Column<bool>(type: "boolean", nullable: false),
                    is_became_final_doc = table.Column<bool>(type: "boolean", nullable: false),
                    act_result_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    selection_protokol_state_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_session_act_selection_protokol", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protokol_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protokol_common_law_unit_selecte~",
                        column: x => x.selected_lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protokol_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "case_session_act_selection_protocol_acts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_session_act_selection_protocol_id = table.Column<int>(type: "integer", nullable: false),
                    case_session_act_id = table.Column<int>(type: "integer", nullable: false),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    is_final = table.Column<bool>(type: "boolean", nullable: false),
                    is_canceleling = table.Column<bool>(type: "boolean", nullable: false),
                    is_became_final_doc = table.Column<bool>(type: "boolean", nullable: false),
                    act_result_id = table.Column<int>(type: "integer", nullable: true),
                    is_selected = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_session_act_selection_protocol_acts", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protocol_acts_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protocol_acts_case_session_act_c~",
                        column: x => x.case_session_act_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protocol_acts_case_session_act_s~",
                        column: x => x.case_session_act_selection_protocol_id,
                        principalTable: "case_session_act_selection_protokol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protocol_acts_identity_users_use~",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_session_act_selection_protocol_acts_nom_act_result_act~",
                        column: x => x.act_result_id,
                        principalTable: "nom_act_result",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protocol_acts_act_result_id",
                table: "case_session_act_selection_protocol_acts",
                column: "act_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protocol_acts_case_id",
                table: "case_session_act_selection_protocol_acts",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protocol_acts_case_session_act_id",
                table: "case_session_act_selection_protocol_acts",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protocol_acts_case_session_act_s~",
                table: "case_session_act_selection_protocol_acts",
                column: "case_session_act_selection_protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protocol_acts_user_id",
                table: "case_session_act_selection_protocol_acts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protokol_court_id",
                table: "case_session_act_selection_protokol",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protokol_selected_lawunit_id",
                table: "case_session_act_selection_protokol",
                column: "selected_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_selection_protokol_user_id",
                table: "case_session_act_selection_protokol",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_session_act_selection_protocol_acts");

            migrationBuilder.DropTable(
                name: "case_session_act_selection_protokol");
        }
    }
}
