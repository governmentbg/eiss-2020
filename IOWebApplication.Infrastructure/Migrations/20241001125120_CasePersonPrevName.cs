using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CasePersonPrevName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "not_punished",
                table: "case_person_crimes",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "case_person_sentence_punishment_measure",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_person_sentence_punishment_id = table.Column<int>(type: "integer", nullable: false),
                    case_person_measure_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_person_sentence_punishment_measure", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_person_sentence_punishment_measure_case_person_measure~",
                        column: x => x.case_person_measure_id,
                        principalTable: "case_person_measures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_person_sentence_punishment_measure_case_person_sentenc~",
                        column: x => x.case_person_sentence_punishment_id,
                        principalTable: "case_person_sentence_punishment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_person_sentence_punishment_measure_identity_users_user~",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "nom_person_prev_names_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_person_prev_names_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "case_person_prev_name",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    case_person_id = table.Column<int>(type: "integer", nullable: false),
                    prev_name_type_id = table.Column<int>(type: "integer", nullable: false),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    uic = table.Column<string>(type: "text", nullable: true),
                    uic_type_id = table.Column<int>(type: "integer", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    family_name = table.Column<string>(type: "text", nullable: true),
                    family_2_name = table.Column<string>(type: "text", nullable: true),
                    full_name = table.Column<string>(type: "text", nullable: true),
                    department_name = table.Column<string>(type: "text", nullable: true),
                    latin_name = table.Column<string>(type: "text", nullable: true),
                    is_deceased = table.Column<bool>(type: "boolean", nullable: true),
                    date_deceased = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    person_source_type = table.Column<int>(type: "integer", nullable: true),
                    person_source_id = table.Column<long>(type: "bigint", nullable: true),
                    person_source_code = table.Column<string>(type: "text", nullable: true),
                    gender_id = table.Column<int>(type: "integer", nullable: true),
                    person_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_person_prev_name", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_person_prev_name_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_person_prev_name_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_person_prev_name_common_person_person_id",
                        column: x => x.person_id,
                        principalTable: "common_person",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_person_prev_name_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_person_prev_name_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_case_person_prev_name_nom_person_prev_names_type_prev_name_~",
                        column: x => x.prev_name_type_id,
                        principalTable: "nom_person_prev_names_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_person_prev_name_nom_uic_type_uic_type_id",
                        column: x => x.uic_type_id,
                        principalTable: "nom_uic_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_case_id",
                table: "case_person_prev_name",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_case_person_id",
                table: "case_person_prev_name",
                column: "case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_person_id",
                table: "case_person_prev_name",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_prev_name_type_id",
                table: "case_person_prev_name",
                column: "prev_name_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_uic_type_id",
                table: "case_person_prev_name",
                column: "uic_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_user_expired_id",
                table: "case_person_prev_name",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_user_id",
                table: "case_person_prev_name",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_punishment_measure_case_person_measure~",
                table: "case_person_sentence_punishment_measure",
                column: "case_person_measure_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_punishment_measure_case_person_sentenc~",
                table: "case_person_sentence_punishment_measure",
                column: "case_person_sentence_punishment_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_punishment_measure_user_id",
                table: "case_person_sentence_punishment_measure",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_person_prev_name");

            migrationBuilder.DropTable(
                name: "case_person_sentence_punishment_measure");

            migrationBuilder.DropTable(
                name: "nom_person_prev_names_type");

            migrationBuilder.DropColumn(
                name: "not_punished",
                table: "case_person_crimes");
        }
    }
}
