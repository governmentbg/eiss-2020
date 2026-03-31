using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SelectionProtocolSubstitutionAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_selection_protokol_substitution",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: false),
                    court_group_id = table.Column<int>(type: "integer", nullable: false),
                    selection_mode_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    selection_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    declare_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    substitution_from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    substitution_to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    substituted_lawunit_id = table.Column<int>(type: "integer", nullable: false),
                    selected_lawunit_id = table.Column<int>(type: "integer", nullable: true),
                    selection_protokol_state_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_selection_protokol_substitution", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_common_court_group_cou~",
                        column: x => x.court_group_id,
                        principalTable: "common_court_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_common_law_unit_select~",
                        column: x => x.selected_lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_common_law_unit_substi~",
                        column: x => x.substituted_lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_nom_selection_mode_sel~",
                        column: x => x.selection_mode_id,
                        principalTable: "nom_selection_mode",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_nom_selection_protokol~",
                        column: x => x.selection_protokol_state_id,
                        principalTable: "nom_selection_protokol_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "case_selection_protokol_substitution_lawunit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_selection_protokol_id = table.Column<int>(type: "integer", nullable: false),
                    lawunit_id = table.Column<int>(type: "integer", nullable: false),
                    load_index = table.Column<int>(type: "integer", nullable: false),
                    case_count = table.Column<int>(type: "integer", nullable: false),
                    case_court_count = table.Column<int>(type: "integer", nullable: true),
                    state_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_selection_protokol_substitution_lawunit", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_lawunit_case_selection~",
                        column: x => x.case_selection_protokol_id,
                        principalTable: "case_selection_protokol_substitution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_lawunit_common_court_c~",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_lawunit_common_law_uni~",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_lawunit_identity_users~",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_selection_protokol_substitution_lawunit_nom_selection_~",
                        column: x => x.state_id,
                        principalTable: "nom_selection_lawunit_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_court_group_id",
                table: "case_selection_protokol_substitution",
                column: "court_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_court_id",
                table: "case_selection_protokol_substitution",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_selected_lawunit_id",
                table: "case_selection_protokol_substitution",
                column: "selected_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_selection_mode_id",
                table: "case_selection_protokol_substitution",
                column: "selection_mode_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_selection_protokol_sta~",
                table: "case_selection_protokol_substitution",
                column: "selection_protokol_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_substituted_lawunit_id",
                table: "case_selection_protokol_substitution",
                column: "substituted_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_user_id",
                table: "case_selection_protokol_substitution",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_lawunit_case_selection~",
                table: "case_selection_protokol_substitution_lawunit",
                column: "case_selection_protokol_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_lawunit_court_id",
                table: "case_selection_protokol_substitution_lawunit",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_lawunit_lawunit_id",
                table: "case_selection_protokol_substitution_lawunit",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_lawunit_state_id",
                table: "case_selection_protokol_substitution_lawunit",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_selection_protokol_substitution_lawunit_user_id",
                table: "case_selection_protokol_substitution_lawunit",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_selection_protokol_substitution_lawunit");

            migrationBuilder.DropTable(
                name: "case_selection_protokol_substitution");
        }
    }
}
