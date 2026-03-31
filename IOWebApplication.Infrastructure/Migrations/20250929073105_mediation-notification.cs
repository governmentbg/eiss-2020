using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mediationnotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mediation_notification_id",
                table: "delivery_item",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "mediation_notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_case_session_id = table.Column<int>(type: "integer", nullable: false),
                    case_person_id = table.Column<int>(type: "integer", nullable: false),
                    case_person_link_id = table.Column<int>(type: "integer", nullable: true),
                    is_multi_link = table.Column<bool>(type: "boolean", nullable: true),
                    notification_address_id = table.Column<long>(type: "bigint", nullable: true),
                    case_person_address_id = table.Column<long>(type: "bigint", nullable: true),
                    notification_type_id = table.Column<int>(type: "integer", nullable: true),
                    reg_number = table.Column<string>(type: "text", nullable: true),
                    reg_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notification_number = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    notification_delivery_group_id = table.Column<int>(type: "integer", nullable: true),
                    notification_state_id = table.Column<int>(type: "integer", nullable: false),
                    delivery_reason_id = table.Column<int>(type: "integer", nullable: true),
                    date_send = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    date_accepted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_official_notification = table.Column<bool>(type: "boolean", nullable: false),
                    html_template_id = table.Column<int>(type: "integer", nullable: true),
                    delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_info = table.Column<string>(type: "text", nullable: true),
                    to_court_id = table.Column<int>(type: "integer", nullable: true),
                    lawunit_id = table.Column<int>(type: "integer", nullable: true),
                    delivery_area_id = table.Column<int>(type: "integer", nullable: true),
                    delivery_oper_id = table.Column<int>(type: "integer", nullable: true),
                    return_info = table.Column<string>(type: "text", nullable: true),
                    return_document_id = table.Column<long>(type: "bigint", nullable: true),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    date_print = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    have_appendix = table.Column<bool>(type: "boolean", nullable: true),
                    is_from_email = table.Column<bool>(type: "boolean", nullable: true),
                    notification_person_name = table.Column<string>(type: "text", nullable: true),
                    notification_person_role = table.Column<string>(type: "text", nullable: true),
                    notification_link_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_notification_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_notification_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_notification_common_address_notification_address_~",
                        column: x => x.notification_address_id,
                        principalTable: "common_address",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_common_html_template_html_template_id",
                        column: x => x.html_template_id,
                        principalTable: "common_html_template",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_mediation_case_session_mediation_cas~",
                        column: x => x.mediation_case_session_id,
                        principalTable: "mediation_case_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_notification_nom_notification_delivery_group_noti~",
                        column: x => x.notification_delivery_group_id,
                        principalTable: "nom_notification_delivery_group",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_nom_notification_state_notification_~",
                        column: x => x.notification_state_id,
                        principalTable: "nom_notification_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_notification_nom_notification_type_notification_t~",
                        column: x => x.notification_type_id,
                        principalTable: "nom_notification_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mediation_notification_mlink",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: true),
                    mediation_notification_id = table.Column<int>(type: "integer", nullable: false),
                    case_person_id = table.Column<int>(type: "integer", nullable: true),
                    case_person_summoned_id = table.Column<int>(type: "integer", nullable: true),
                    case_person_link_id = table.Column<int>(type: "integer", nullable: true),
                    person_name = table.Column<string>(type: "text", nullable: true),
                    person_role = table.Column<string>(type: "text", nullable: true),
                    is_checked = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_notification_mlink", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_notification_mlink_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_mlink_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_mlink_case_person_case_person_summon~",
                        column: x => x.case_person_summoned_id,
                        principalTable: "case_person",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_mlink_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_notification_mlink_mediation_notification_mediati~",
                        column: x => x.mediation_notification_id,
                        principalTable: "mediation_notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_mediation_notification_id",
                table: "delivery_item",
                column: "mediation_notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_case_id",
                table: "mediation_notification",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_case_person_id",
                table: "mediation_notification",
                column: "case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_court_id",
                table: "mediation_notification",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_html_template_id",
                table: "mediation_notification",
                column: "html_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_lawunit_id",
                table: "mediation_notification",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_mediation_case_session_id",
                table: "mediation_notification",
                column: "mediation_case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_notification_address_id",
                table: "mediation_notification",
                column: "notification_address_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_notification_delivery_group_id",
                table: "mediation_notification",
                column: "notification_delivery_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_notification_state_id",
                table: "mediation_notification",
                column: "notification_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_notification_type_id",
                table: "mediation_notification",
                column: "notification_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_user_expired_id",
                table: "mediation_notification",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_user_id",
                table: "mediation_notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_mlink_case_id",
                table: "mediation_notification_mlink",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_mlink_case_person_id",
                table: "mediation_notification_mlink",
                column: "case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_mlink_case_person_summoned_id",
                table: "mediation_notification_mlink",
                column: "case_person_summoned_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_mlink_court_id",
                table: "mediation_notification_mlink",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_notification_mlink_mediation_notification_id",
                table: "mediation_notification_mlink",
                column: "mediation_notification_id");

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_item_mediation_notification_mediation_notification~",
                table: "delivery_item",
                column: "mediation_notification_id",
                principalTable: "mediation_notification",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_delivery_item_mediation_notification_mediation_notification~",
                table: "delivery_item");

            migrationBuilder.DropTable(
                name: "mediation_notification_mlink");

            migrationBuilder.DropTable(
                name: "mediation_notification");

            migrationBuilder.DropIndex(
                name: "IX_delivery_item_mediation_notification_id",
                table: "delivery_item");

            migrationBuilder.DropColumn(
                name: "mediation_notification_id",
                table: "delivery_item");
        }
    }
}
