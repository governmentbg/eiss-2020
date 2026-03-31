using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class SessionResultBaseRuleAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_session_result_base_rule",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    session_result_base_id = table.Column<int>(nullable: false),
                    case_group_id = table.Column<int>(nullable: false),
                    date_start = table.Column<DateTime>(nullable: true),
                    date_end = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_session_result_base_rule", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_session_result_base_rule_nom_case_group_case_group_id",
                        column: x => x.case_group_id,
                        principalTable: "nom_case_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_session_result_base_rule_nom_session_result_base_sessio~",
                        column: x => x.session_result_base_id,
                        principalTable: "nom_session_result_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_session_result_base_rule_case_group_id",
                table: "nom_session_result_base_rule",
                column: "case_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_session_result_base_rule_session_result_base_id",
                table: "nom_session_result_base_rule",
                column: "session_result_base_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_session_result_base_rule");
        }
    }
}
