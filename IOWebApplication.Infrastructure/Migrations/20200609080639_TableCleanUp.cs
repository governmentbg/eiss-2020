using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class TableCleanUp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_common_compartment_compartment_id",
                table: "case_session");

            migrationBuilder.DropTable(
                name: "common_compartment_lawunit");

            migrationBuilder.DropTable(
                name: "common_external_data");

            migrationBuilder.DropTable(
                name: "common_compartment");

            migrationBuilder.DropIndex(
                name: "IX_case_session_compartment_id",
                table: "case_session");

            migrationBuilder.DropColumn(
                name: "add_count_judge",
                table: "nom_case_type");

            migrationBuilder.DropColumn(
                name: "add_count_jury",
                table: "nom_case_type");

            migrationBuilder.DropColumn(
                name: "count_judge",
                table: "nom_case_type");

            migrationBuilder.DropColumn(
                name: "count_jury",
                table: "nom_case_type");

            migrationBuilder.DropColumn(
                name: "reserve_count_judge",
                table: "nom_case_type");

            migrationBuilder.DropColumn(
                name: "reserve_count_jury",
                table: "nom_case_type");

            migrationBuilder.DropColumn(
                name: "compartment_id",
                table: "case_session_h");

            migrationBuilder.DropColumn(
                name: "compartment_id",
                table: "case_session");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "add_count_judge",
                table: "nom_case_type",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "add_count_jury",
                table: "nom_case_type",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "count_judge",
                table: "nom_case_type",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "count_jury",
                table: "nom_case_type",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reserve_count_judge",
                table: "nom_case_type",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reserve_count_jury",
                table: "nom_case_type",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "compartment_id",
                table: "case_session_h",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "compartment_id",
                table: "case_session",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "common_compartment",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_id = table.Column<int>(nullable: false),
                    date_from = table.Column<DateTime>(nullable: false),
                    date_to = table.Column<DateTime>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    label = table.Column<string>(nullable: false),
                    lawunit_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_compartment", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_compartment_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_compartment_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_external_data",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    content = table.Column<byte[]>(nullable: true),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    integration_type_id = table.Column<int>(nullable: false),
                    method_name = table.Column<string>(nullable: true),
                    source_id = table.Column<string>(nullable: true),
                    source_type = table.Column<int>(nullable: false),
                    target_class_name = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_external_data", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_external_data_nom_integration_type_integration_type_~",
                        column: x => x.integration_type_id,
                        principalTable: "nom_integration_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_external_data_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "common_compartment_lawunit",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    compartment_id = table.Column<int>(nullable: false),
                    lawunit_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_compartment_lawunit", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_compartment_lawunit_common_compartment_compartment_id",
                        column: x => x.compartment_id,
                        principalTable: "common_compartment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_compartment_lawunit_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_session_compartment_id",
                table: "case_session",
                column: "compartment_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_compartment_court_id",
                table: "common_compartment",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_compartment_lawunit_id",
                table: "common_compartment",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_compartment_lawunit_compartment_id",
                table: "common_compartment_lawunit",
                column: "compartment_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_compartment_lawunit_lawunit_id",
                table: "common_compartment_lawunit",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_external_data_integration_type_id",
                table: "common_external_data",
                column: "integration_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_external_data_user_id",
                table: "common_external_data",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_common_compartment_compartment_id",
                table: "case_session",
                column: "compartment_id",
                principalTable: "common_compartment",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
