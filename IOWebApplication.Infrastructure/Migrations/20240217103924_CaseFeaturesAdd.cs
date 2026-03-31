using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseFeaturesAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "td_act_for_registration",
                table: "case_session_act_h",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "td_act_for_registration",
                table: "case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ispn_case_competence_id",
                table: "case_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ispn_case_competence_id",
                table: "case",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_case_feature",
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
                    all_court_types = table.Column<bool>(nullable: false),
                    all_case_types = table.Column<bool>(nullable: false),
                    all_case_codes = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_case_feature", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_ispn_case_competence",
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
                    date_end = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_ispn_case_competence", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_case_feature_code",
                columns: table => new
                {
                    case_feature_id = table.Column<int>(nullable: false),
                    case_code_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_case_feature_code", x => new { x.case_feature_id, x.case_code_id });
                    table.ForeignKey(
                        name: "FK_nom_case_feature_code_nom_case_code_case_code_id",
                        column: x => x.case_code_id,
                        principalTable: "nom_case_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_case_feature_code_nom_case_feature_case_feature_id",
                        column: x => x.case_feature_id,
                        principalTable: "nom_case_feature",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nom_case_feature_court_type",
                columns: table => new
                {
                    case_feature_id = table.Column<int>(nullable: false),
                    court_type_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_case_feature_court_type", x => new { x.case_feature_id, x.court_type_id });
                    table.ForeignKey(
                        name: "FK_nom_case_feature_court_type_nom_case_feature_case_feature_id",
                        column: x => x.case_feature_id,
                        principalTable: "nom_case_feature",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_case_feature_court_type_nom_court_type_court_type_id",
                        column: x => x.court_type_id,
                        principalTable: "nom_court_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nom_case_feature_type",
                columns: table => new
                {
                    case_feature_id = table.Column<int>(nullable: false),
                    case_type_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_case_feature_type", x => new { x.case_feature_id, x.case_type_id });
                    table.ForeignKey(
                        name: "FK_nom_case_feature_type_nom_case_feature_case_feature_id",
                        column: x => x.case_feature_id,
                        principalTable: "nom_case_feature",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_case_feature_type_nom_case_type_case_type_id",
                        column: x => x.case_type_id,
                        principalTable: "nom_case_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_ispn_case_competence_id",
                table: "case",
                column: "ispn_case_competence_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_feature_code_case_code_id",
                table: "nom_case_feature_code",
                column: "case_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_feature_court_type_court_type_id",
                table: "nom_case_feature_court_type",
                column: "court_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_feature_type_case_type_id",
                table: "nom_case_feature_type",
                column: "case_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_nom_ispn_case_competence_ispn_case_competence_id",
                table: "case",
                column: "ispn_case_competence_id",
                principalTable: "nom_ispn_case_competence",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_nom_ispn_case_competence_ispn_case_competence_id",
                table: "case");

            migrationBuilder.DropTable(
                name: "nom_case_feature_code");

            migrationBuilder.DropTable(
                name: "nom_case_feature_court_type");

            migrationBuilder.DropTable(
                name: "nom_case_feature_type");

            migrationBuilder.DropTable(
                name: "nom_ispn_case_competence");

            migrationBuilder.DropTable(
                name: "nom_case_feature");

            migrationBuilder.DropIndex(
                name: "IX_case_ispn_case_competence_id",
                table: "case");

            migrationBuilder.DropColumn(
                name: "td_act_for_registration",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "td_act_for_registration",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "ispn_case_competence_id",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "ispn_case_competence_id",
                table: "case");
        }
    }
}
