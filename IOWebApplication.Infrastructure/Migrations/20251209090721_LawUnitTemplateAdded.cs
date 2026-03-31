using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LawUnitTemplateAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_lawunit_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: true),
                    lawunit_id = table.Column<int>(type: "integer", nullable: false),
                    case_group_id = table.Column<int>(type: "integer", nullable: true),
                    act_type_id = table.Column<int>(type: "integer", nullable: true),
                    act_main = table.Column<string>(type: "text", nullable: true),
                    act_dispositive = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_lawunit_template", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_lawunit_template_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_lawunit_template_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_lawunit_template_nom_act_type_act_type_id",
                        column: x => x.act_type_id,
                        principalTable: "nom_act_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_lawunit_template_nom_case_group_case_group_id",
                        column: x => x.case_group_id,
                        principalTable: "nom_case_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_lawunit_template_act_type_id",
                table: "common_lawunit_template",
                column: "act_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_lawunit_template_case_group_id",
                table: "common_lawunit_template",
                column: "case_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_lawunit_template_lawunit_id",
                table: "common_lawunit_template",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_lawunit_template_user_id",
                table: "common_lawunit_template",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_lawunit_template");
        }
    }
}
