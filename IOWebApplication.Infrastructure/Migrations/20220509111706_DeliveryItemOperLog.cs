using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DeliveryItemOperLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "delivery_item_oper_log",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    reg_number = table.Column<string>(nullable: true),
                    reg_date = table.Column<DateTime>(nullable: true),
                    case_info = table.Column<string>(nullable: true),
                    delivery_item_id = table.Column<int>(nullable: true),
                    delivery_item_oper_id = table.Column<int>(nullable: true),
                    case_notification_id = table.Column<int>(nullable: true),
                    document_notification_id = table.Column<int>(nullable: true),
                    court_wrt_id = table.Column<int>(nullable: false),
                    from_court_id = table.Column<int>(nullable: true),
                    to_court_id = table.Column<int>(nullable: true),
                    delivery_oper_id = table.Column<int>(nullable: true),
                    delivery_area_id = table.Column<int>(nullable: true),
                    notification_state_id = table.Column<int>(nullable: false),
                    @long = table.Column<string>(name: "long", nullable: true),
                    lat = table.Column<string>(nullable: true),
                    lawunit_id = table.Column<int>(nullable: true),
                    delivery_info = table.Column<string>(nullable: true),
                    delivery_reason_id = table.Column<int>(nullable: true),
                    date_oper = table.Column<DateTime>(nullable: true),
                    screen_label = table.Column<string>(nullable: true),
                    screen_url = table.Column<string>(nullable: true),
                    action = table.Column<string>(nullable: true),
                    is_from_mobile = table.Column<bool>(nullable: true),
                    case_group_id = table.Column<int>(nullable: true),
                    case_type_id = table.Column<int>(nullable: true),
                    person_name = table.Column<string>(nullable: true),
                    address_str = table.Column<string>(nullable: true),
                    NotificationDeliveryGroupId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_item_oper_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_common_court_court_wrt_id",
                        column: x => x.court_wrt_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_delivery_area_delivery_area_id",
                        column: x => x.delivery_area_id,
                        principalTable: "delivery_area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_delivery_item_delivery_item_id",
                        column: x => x.delivery_item_id,
                        principalTable: "delivery_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_delivery_item_oper_delivery_item_ope~",
                        column: x => x.delivery_item_oper_id,
                        principalTable: "delivery_item_oper",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_nom_delivery_oper_delivery_oper_id",
                        column: x => x.delivery_oper_id,
                        principalTable: "nom_delivery_oper",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_nom_delivery_reason_delivery_reason_~",
                        column: x => x.delivery_reason_id,
                        principalTable: "nom_delivery_reason",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_nom_notification_state_notification_~",
                        column: x => x.notification_state_id,
                        principalTable: "nom_notification_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_delivery_item_oper_log_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_court_wrt_id",
                table: "delivery_item_oper_log",
                column: "court_wrt_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_delivery_area_id",
                table: "delivery_item_oper_log",
                column: "delivery_area_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_delivery_item_id",
                table: "delivery_item_oper_log",
                column: "delivery_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_delivery_item_oper_id",
                table: "delivery_item_oper_log",
                column: "delivery_item_oper_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_delivery_oper_id",
                table: "delivery_item_oper_log",
                column: "delivery_oper_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_delivery_reason_id",
                table: "delivery_item_oper_log",
                column: "delivery_reason_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_lawunit_id",
                table: "delivery_item_oper_log",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_notification_state_id",
                table: "delivery_item_oper_log",
                column: "notification_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_oper_log_user_id",
                table: "delivery_item_oper_log",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_item_oper_log");
        }
    }
}
