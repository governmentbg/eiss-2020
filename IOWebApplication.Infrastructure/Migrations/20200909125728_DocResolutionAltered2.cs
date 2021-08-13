using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocResolutionAltered2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_resolution_state",
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
                    table.PrimaryKey("PK_nom_resolution_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_resolution_type",
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
                    table.PrimaryKey("PK_nom_resolution_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_resolution",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: false),
                    document_id = table.Column<long>(nullable: false),
                    decision_type_id = table.Column<int>(nullable: false),
                    reg_number = table.Column<string>(nullable: true),
                    reg_date = table.Column<DateTime>(nullable: false),
                    declared_date = table.Column<DateTime>(nullable: true),
                    judge_decision_lawunit_id = table.Column<int>(nullable: false),
                    judge_decision_user_id = table.Column<string>(nullable: true),
                    user_decision_id = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    document_decision_state_id = table.Column<int>(nullable: false),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_resolution", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_resolution_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_resolution_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_resolution_common_law_unit_judge_decision_lawunit_~",
                        column: x => x.judge_decision_lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_resolution_identity_users_judge_decision_user_id",
                        column: x => x.judge_decision_user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_resolution_nom_resolution_state_document_decision_~",
                        column: x => x.document_decision_state_id,
                        principalTable: "nom_resolution_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_resolution_nom_resolution_type_decision_type_id",
                        column: x => x.decision_type_id,
                        principalTable: "nom_resolution_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_resolution_identity_users_user_decision_id",
                        column: x => x.user_decision_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_resolution_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_resolution_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_court_id",
                table: "document_resolution",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_document_id",
                table: "document_resolution",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_judge_decision_lawunit_id",
                table: "document_resolution",
                column: "judge_decision_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_judge_decision_user_id",
                table: "document_resolution",
                column: "judge_decision_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_document_decision_state_id",
                table: "document_resolution",
                column: "document_decision_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_decision_type_id",
                table: "document_resolution",
                column: "decision_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_user_decision_id",
                table: "document_resolution",
                column: "user_decision_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_user_expired_id",
                table: "document_resolution",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_resolution_user_id",
                table: "document_resolution",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_resolution");

            migrationBuilder.DropTable(
                name: "nom_resolution_state");

            migrationBuilder.DropTable(
                name: "nom_resolution_type");
        }
    }
}
