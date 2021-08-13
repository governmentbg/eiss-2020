using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "document_notification_id",
                table: "delivery_item",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "document_person_link",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: true),
                    document_id = table.Column<long>(nullable: false),
                    document_person_id = table.Column<long>(nullable: false),
                    document_person_rel_id = table.Column<long>(nullable: false),
                    document_person_second_rel_id = table.Column<long>(nullable: true),
                    link_direction_id = table.Column<int>(nullable: false),
                    link_direction_second_id = table.Column<int>(nullable: true),
                    date_from = table.Column<DateTime>(nullable: false),
                    date_to = table.Column<DateTime>(nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_person_link", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_person_link_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_person_link_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_person_link_document_person_document_person_id",
                        column: x => x.document_person_id,
                        principalTable: "document_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_person_link_document_person_document_person_rel_id",
                        column: x => x.document_person_rel_id,
                        principalTable: "document_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_person_link_document_person_document_person_second~",
                        column: x => x.document_person_second_rel_id,
                        principalTable: "document_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_person_link_nom_link_direction_link_direction_id",
                        column: x => x.link_direction_id,
                        principalTable: "nom_link_direction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_person_link_nom_link_direction_link_direction_seco~",
                        column: x => x.link_direction_second_id,
                        principalTable: "nom_link_direction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_person_link_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_notification",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: true),
                    document_id = table.Column<long>(nullable: true),
                    document_resolution_id = table.Column<long>(nullable: true),
                    document_person_id = table.Column<long>(nullable: true),
                    case_person_link_id = table.Column<int>(nullable: true),
                    notification_address_id = table.Column<long>(nullable: true),
                    notification_type_id = table.Column<int>(nullable: true),
                    reg_number = table.Column<string>(nullable: true),
                    reg_date = table.Column<DateTime>(nullable: false),
                    notification_number = table.Column<int>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    notification_delivery_group_id = table.Column<int>(nullable: true),
                    notification_state_id = table.Column<int>(nullable: false),
                    delivery_reason_id = table.Column<int>(nullable: true),
                    date_send = table.Column<DateTime>(nullable: true),
                    date_accepted = table.Column<DateTime>(nullable: true),
                    is_official_notification = table.Column<bool>(nullable: false),
                    html_template_id = table.Column<int>(nullable: true),
                    delivery_date = table.Column<DateTime>(nullable: true),
                    delivery_info = table.Column<string>(nullable: true),
                    to_court_id = table.Column<int>(nullable: true),
                    lawunit_id = table.Column<int>(nullable: true),
                    delivery_area_id = table.Column<int>(nullable: true),
                    delivery_oper_id = table.Column<int>(nullable: true),
                    return_info = table.Column<string>(nullable: true),
                    return_document_id = table.Column<long>(nullable: true),
                    return_date = table.Column<DateTime>(nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true),
                    date_print = table.Column<DateTime>(nullable: true),
                    have_appendix = table.Column<bool>(nullable: true),
                    is_from_email = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_notification_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_document_person_document_person_id",
                        column: x => x.document_person_id,
                        principalTable: "document_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_document_person_link_case_person_link~",
                        column: x => x.case_person_link_id,
                        principalTable: "document_person_link",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_document_resolution_document_resoluti~",
                        column: x => x.document_resolution_id,
                        principalTable: "document_resolution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_common_html_template_html_template_id",
                        column: x => x.html_template_id,
                        principalTable: "common_html_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_common_address_notification_address_id",
                        column: x => x.notification_address_id,
                        principalTable: "common_address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_nom_notification_delivery_group_notif~",
                        column: x => x.notification_delivery_group_id,
                        principalTable: "nom_notification_delivery_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_nom_notification_state_notification_s~",
                        column: x => x.notification_state_id,
                        principalTable: "nom_notification_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_notification_nom_notification_type_notification_ty~",
                        column: x => x.notification_type_id,
                        principalTable: "nom_notification_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_document_notification_id",
                table: "delivery_item",
                column: "document_notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_court_id",
                table: "document_notification",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_document_id",
                table: "document_notification",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_document_person_id",
                table: "document_notification",
                column: "document_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_case_person_link_id",
                table: "document_notification",
                column: "case_person_link_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_document_resolution_id",
                table: "document_notification",
                column: "document_resolution_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_html_template_id",
                table: "document_notification",
                column: "html_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_notification_address_id",
                table: "document_notification",
                column: "notification_address_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_notification_delivery_group_id",
                table: "document_notification",
                column: "notification_delivery_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_notification_state_id",
                table: "document_notification",
                column: "notification_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_notification_type_id",
                table: "document_notification",
                column: "notification_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_user_expired_id",
                table: "document_notification",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_user_id",
                table: "document_notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_court_id",
                table: "document_person_link",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_document_id",
                table: "document_person_link",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_document_person_id",
                table: "document_person_link",
                column: "document_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_document_person_rel_id",
                table: "document_person_link",
                column: "document_person_rel_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_document_person_second_rel_id",
                table: "document_person_link",
                column: "document_person_second_rel_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_link_direction_id",
                table: "document_person_link",
                column: "link_direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_link_direction_second_id",
                table: "document_person_link",
                column: "link_direction_second_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_link_user_id",
                table: "document_person_link",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_item_document_notification_document_notification_id",
                table: "delivery_item",
                column: "document_notification_id",
                principalTable: "document_notification",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_delivery_item_document_notification_document_notification_id",
                table: "delivery_item");

            migrationBuilder.DropTable(
                name: "document_notification");

            migrationBuilder.DropTable(
                name: "document_person_link");

            migrationBuilder.DropIndex(
                name: "IX_delivery_item_document_notification_id",
                table: "delivery_item");

            migrationBuilder.DropColumn(
                name: "document_notification_id",
                table: "delivery_item");
        }
    }
}
