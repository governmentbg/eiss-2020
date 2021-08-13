using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddTableCaseLoadElementTypeRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_case_load_element_type_rule",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    order_number = table.Column<int>(nullable: false),
                    code = table.Column<string>(nullable: true),
                    label = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    date_start = table.Column<DateTime>(nullable: false),
                    date_end = table.Column<DateTime>(nullable: true),
                    case_load_element_type_id = table.Column<int>(nullable: false),
                    session_type_id = table.Column<int>(nullable: false),
                    session_result_id = table.Column<int>(nullable: false),
                    act_type_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_case_load_element_type_rule", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_case_load_element_type_rule_nom_act_type_act_type_id",
                        column: x => x.act_type_id,
                        principalTable: "nom_act_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_case_load_element_type_rule_nom_case_load_element_type_~",
                        column: x => x.case_load_element_type_id,
                        principalTable: "nom_case_load_element_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_case_load_element_type_rule_nom_session_result_session_~",
                        column: x => x.session_result_id,
                        principalTable: "nom_session_result",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_case_load_element_type_rule_nom_session_type_session_ty~",
                        column: x => x.session_type_id,
                        principalTable: "nom_session_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_rule_act_type_id",
                table: "nom_case_load_element_type_rule",
                column: "act_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_rule_case_load_element_type_id",
                table: "nom_case_load_element_type_rule",
                column: "case_load_element_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_rule_session_result_id",
                table: "nom_case_load_element_type_rule",
                column: "session_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_rule_session_type_id",
                table: "nom_case_load_element_type_rule",
                column: "session_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_case_load_element_type_rule");
        }
    }
}
