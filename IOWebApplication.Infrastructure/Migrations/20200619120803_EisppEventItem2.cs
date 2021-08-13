using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EisppEventItem2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eispp_event_item",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    event_type = table.Column<int>(nullable: false),
                    event_from_id = table.Column<int>(nullable: true),
                    mq_epep_id = table.Column<long>(nullable: true),
                    case_id = table.Column<int>(nullable: false),
                    case_session_id = table.Column<int>(nullable: true),
                    case_session_act_id = table.Column<int>(nullable: true),
                    case_preson_id = table.Column<int>(nullable: true),
                    preson_old_measure_id = table.Column<int>(nullable: true),
                    preson_measure_id = table.Column<int>(nullable: true),
                    punishment_id = table.Column<int>(nullable: true),
                    source_type = table.Column<int>(nullable: false),
                    source_id = table.Column<long>(nullable: false),
                    request_data = table.Column<string>(type: "jsonb", nullable: true),
                    response_data = table.Column<string>(type: "jsonb", nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eispp_event_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_eispp_event_item_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eispp_event_item_case_person_case_preson_id",
                        column: x => x.case_preson_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_eispp_event_item_case_session_act_case_session_act_id",
                        column: x => x.case_session_act_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_eispp_event_item_case_session_case_session_id",
                        column: x => x.case_session_id,
                        principalTable: "case_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_eispp_event_item_common_mq_epep_mq_epep_id",
                        column: x => x.mq_epep_id,
                        principalTable: "common_mq_epep",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_eispp_event_item_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_eispp_event_item_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eispp_event_item_case_id",
                table: "eispp_event_item",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_eispp_event_item_case_preson_id",
                table: "eispp_event_item",
                column: "case_preson_id");

            migrationBuilder.CreateIndex(
                name: "IX_eispp_event_item_case_session_act_id",
                table: "eispp_event_item",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_eispp_event_item_case_session_id",
                table: "eispp_event_item",
                column: "case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_eispp_event_item_mq_epep_id",
                table: "eispp_event_item",
                column: "mq_epep_id");

            migrationBuilder.CreateIndex(
                name: "IX_eispp_event_item_user_expired_id",
                table: "eispp_event_item",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_eispp_event_item_user_id",
                table: "eispp_event_item",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eispp_event_item");
        }
    }
}
