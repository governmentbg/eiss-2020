using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ElectionGroupAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_election_person_dismissal_type",
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
                    table.PrimaryKey("PK_nom_election_person_dismissal_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_election_person_state",
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
                    table.PrimaryKey("PK_nom_election_person_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_election_type",
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
                    table.PrimaryKey("PK_nom_election_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "election_group",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_id = table.Column<int>(nullable: false),
                    document_id = table.Column<long>(nullable: false),
                    label = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    election_type_id = table.Column<int>(nullable: false),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_election_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_election_group_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_group_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_group_nom_election_type_election_type_id",
                        column: x => x.election_type_id,
                        principalTable: "nom_election_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_group_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "election_log",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ElectionGroupId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    DateWrt = table.Column<DateTime>(nullable: false),
                    PersonGid = table.Column<Guid>(nullable: true),
                    LawunitId = table.Column<int>(nullable: true),
                    Operation = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_election_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_election_log_election_group_ElectionGroupId",
                        column: x => x.ElectionGroupId,
                        principalTable: "election_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_log_identity_users_UserId",
                        column: x => x.UserId,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "election_person",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    election_group_id = table.Column<int>(nullable: false),
                    election_protocol_id = table.Column<int>(nullable: true),
                    person_gid = table.Column<Guid>(nullable: false),
                    lawunit_id = table.Column<int>(nullable: false),
                    lawunit_fullname = table.Column<string>(nullable: true),
                    court_id = table.Column<int>(nullable: false),
                    user_added_id = table.Column<string>(nullable: true),
                    date_added = table.Column<DateTime>(nullable: false),
                    election_person_state_id = table.Column<int>(nullable: false),
                    description_state = table.Column<string>(nullable: true),
                    election_person_dismissal_type_id = table.Column<int>(nullable: true),
                    user_removed_id = table.Column<string>(nullable: true),
                    date_removed_id = table.Column<DateTime>(nullable: true),
                    description_removed = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_election_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_election_person_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_person_election_group_election_group_id",
                        column: x => x.election_group_id,
                        principalTable: "election_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_person_nom_election_person_state_election_person_s~",
                        column: x => x.election_person_state_id,
                        principalTable: "nom_election_person_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_person_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_person_identity_users_user_added_id",
                        column: x => x.user_added_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_election_person_identity_users_user_removed_id",
                        column: x => x.user_removed_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "election_protocol",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    election_group_id = table.Column<int>(nullable: false),
                    selected_election_person_id = table.Column<int>(nullable: true),
                    selected_lawunit_id = table.Column<int>(nullable: true),
                    selected_lawunit_fullname = table.Column<string>(nullable: true),
                    selected_lawunit_court_id = table.Column<int>(nullable: true),
                    prev_lawunit_id = table.Column<int>(nullable: true),
                    prev_lawunit_fullname = table.Column<string>(nullable: true),
                    prev_lawunit_court_id = table.Column<int>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    user_added_id = table.Column<string>(nullable: true),
                    date_added = table.Column<DateTime>(nullable: false),
                    date_signed = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_election_protocol", x => x.id);
                    table.ForeignKey(
                        name: "FK_election_protocol_election_group_election_group_id",
                        column: x => x.election_group_id,
                        principalTable: "election_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_election_protocol_common_court_prev_lawunit_court_id",
                        column: x => x.prev_lawunit_court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_election_protocol_common_law_unit_prev_lawunit_id",
                        column: x => x.prev_lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_election_protocol_election_person_selected_election_person_~",
                        column: x => x.selected_election_person_id,
                        principalTable: "election_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_election_protocol_common_court_selected_lawunit_court_id",
                        column: x => x.selected_lawunit_court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_election_protocol_common_law_unit_selected_lawunit_id",
                        column: x => x.selected_lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_election_protocol_identity_users_user_added_id",
                        column: x => x.user_added_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_election_group_court_id",
                table: "election_group",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_group_document_id",
                table: "election_group",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_group_election_type_id",
                table: "election_group",
                column: "election_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_group_user_id",
                table: "election_group",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_log_ElectionGroupId",
                table: "election_log",
                column: "ElectionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_election_log_UserId",
                table: "election_log",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_election_person_court_id",
                table: "election_person",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_person_election_group_id",
                table: "election_person",
                column: "election_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_person_election_person_state_id",
                table: "election_person",
                column: "election_person_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_person_election_protocol_id",
                table: "election_person",
                column: "election_protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_person_lawunit_id",
                table: "election_person",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_person_user_added_id",
                table: "election_person",
                column: "user_added_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_person_user_removed_id",
                table: "election_person",
                column: "user_removed_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_election_group_id",
                table: "election_protocol",
                column: "election_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_prev_lawunit_court_id",
                table: "election_protocol",
                column: "prev_lawunit_court_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_prev_lawunit_id",
                table: "election_protocol",
                column: "prev_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_selected_election_person_id",
                table: "election_protocol",
                column: "selected_election_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_selected_lawunit_court_id",
                table: "election_protocol",
                column: "selected_lawunit_court_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_selected_lawunit_id",
                table: "election_protocol",
                column: "selected_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_protocol_user_added_id",
                table: "election_protocol",
                column: "user_added_id");

            migrationBuilder.AddForeignKey(
                name: "FK_election_person_election_protocol_election_protocol_id",
                table: "election_person",
                column: "election_protocol_id",
                principalTable: "election_protocol",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_election_group_nom_election_type_election_type_id",
                table: "election_group");

            migrationBuilder.DropForeignKey(
                name: "FK_election_person_election_group_election_group_id",
                table: "election_person");

            migrationBuilder.DropForeignKey(
                name: "FK_election_protocol_election_group_election_group_id",
                table: "election_protocol");

            migrationBuilder.DropForeignKey(
                name: "FK_election_person_nom_election_person_state_election_person_s~",
                table: "election_person");

            migrationBuilder.DropForeignKey(
                name: "FK_election_person_election_protocol_election_protocol_id",
                table: "election_person");

            migrationBuilder.DropTable(
                name: "election_log");

            migrationBuilder.DropTable(
                name: "nom_election_person_dismissal_type");

            migrationBuilder.DropTable(
                name: "nom_election_type");

            migrationBuilder.DropTable(
                name: "election_group");

            migrationBuilder.DropTable(
                name: "nom_election_person_state");

            migrationBuilder.DropTable(
                name: "election_protocol");

            migrationBuilder.DropTable(
                name: "election_person");
        }
    }
}
