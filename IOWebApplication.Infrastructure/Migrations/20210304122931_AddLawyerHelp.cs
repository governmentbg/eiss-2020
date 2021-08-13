using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddLawyerHelp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_lawyer_help_base",
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
                    table.PrimaryKey("PK_nom_lawyer_help_base", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_lawyer_help_type",
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
                    table.PrimaryKey("PK_nom_lawyer_help_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "case_lawyer_help",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: true),
                    case_id = table.Column<int>(nullable: false),
                    lawyer_help_base_id = table.Column<int>(nullable: false),
                    lawyer_help_type_id = table.Column<int>(nullable: false),
                    case_session_act_id = table.Column<int>(nullable: false),
                    has_interest_conflict = table.Column<bool>(nullable: false),
                    prev_defender_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_lawyer_help", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_nom_lawyer_help_base_lawyer_help_base_id",
                        column: x => x.lawyer_help_base_id,
                        principalTable: "nom_lawyer_help_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_nom_lawyer_help_type_lawyer_help_type_id",
                        column: x => x.lawyer_help_type_id,
                        principalTable: "nom_lawyer_help_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_lawyer_help_other_lawyer",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    case_lawyer_help_id = table.Column<int>(nullable: false),
                    case_person_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_lawyer_help_other_lawyer", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_other_lawyer_case_lawyer_help_case_lawyer_~",
                        column: x => x.case_lawyer_help_id,
                        principalTable: "case_lawyer_help",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_other_lawyer_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "case_lawyer_help_person",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    case_lawyer_help_id = table.Column<int>(nullable: false),
                    case_person_id = table.Column<int>(nullable: false),
                    assigned_lawyer_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_lawyer_help_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_person_case_person_assigned_lawyer_id",
                        column: x => x.assigned_lawyer_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_person_case_lawyer_help_case_lawyer_help_id",
                        column: x => x.case_lawyer_help_id,
                        principalTable: "case_lawyer_help",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_person_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_case_id",
                table: "case_lawyer_help",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_court_id",
                table: "case_lawyer_help",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_lawyer_help_base_id",
                table: "case_lawyer_help",
                column: "lawyer_help_base_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_lawyer_help_type_id",
                table: "case_lawyer_help",
                column: "lawyer_help_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_user_expired_id",
                table: "case_lawyer_help",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_user_id",
                table: "case_lawyer_help",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_other_lawyer_case_lawyer_help_id",
                table: "case_lawyer_help_other_lawyer",
                column: "case_lawyer_help_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_other_lawyer_case_person_id",
                table: "case_lawyer_help_other_lawyer",
                column: "case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_person_assigned_lawyer_id",
                table: "case_lawyer_help_person",
                column: "assigned_lawyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_person_case_lawyer_help_id",
                table: "case_lawyer_help_person",
                column: "case_lawyer_help_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_person_case_person_id",
                table: "case_lawyer_help_person",
                column: "case_person_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_lawyer_help_other_lawyer");

            migrationBuilder.DropTable(
                name: "case_lawyer_help_person");

            migrationBuilder.DropTable(
                name: "case_lawyer_help");

            migrationBuilder.DropTable(
                name: "nom_lawyer_help_base");

            migrationBuilder.DropTable(
                name: "nom_lawyer_help_type");
        }
    }
}
