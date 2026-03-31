using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseSessionActLawunitsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_session_act_lawunit",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_session_act_id = table.Column<int>(type: "integer", nullable: false),
                    judge_role_id = table.Column<int>(type: "integer", nullable: false),
                    lawunit_id = table.Column<int>(type: "integer", nullable: false),
                    lawunit_user_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_session_act_lawunit", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_session_act_lawunit_case_session_act_case_session_act_~",
                        column: x => x.case_session_act_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_lawunit_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_lawunit_identity_users_lawunit_user_id",
                        column: x => x.lawunit_user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_session_act_lawunit_nom_judge_role_judge_role_id",
                        column: x => x.judge_role_id,
                        principalTable: "nom_judge_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_lawunit_case_session_act_id",
                table: "case_session_act_lawunit",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_lawunit_judge_role_id",
                table: "case_session_act_lawunit",
                column: "judge_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_lawunit_lawunit_id",
                table: "case_session_act_lawunit",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_lawunit_lawunit_user_id",
                table: "case_session_act_lawunit",
                column: "lawunit_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_session_act_lawunit");
        }
    }
}
