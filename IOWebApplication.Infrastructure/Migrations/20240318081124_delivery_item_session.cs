using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class delivery_item_session : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_id",
                table: "delivery_item",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_session_id",
                table: "delivery_item",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "notification_delivery_group_id",
                table: "delivery_item",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "prepared_by_id",
                table: "delivery_item",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_case_id",
                table: "delivery_item",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_case_session_id",
                table: "delivery_item",
                column: "case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_notification_delivery_group_id",
                table: "delivery_item",
                column: "notification_delivery_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_item_prepared_by_id",
                table: "delivery_item",
                column: "prepared_by_id");

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_item_case_case_id",
                table: "delivery_item",
                column: "case_id",
                principalTable: "case",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_item_case_session_case_session_id",
                table: "delivery_item",
                column: "case_session_id",
                principalTable: "case_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_item_nom_notification_delivery_group_notification_~",
                table: "delivery_item",
                column: "notification_delivery_group_id",
                principalTable: "nom_notification_delivery_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_item_common_law_unit_prepared_by_id",
                table: "delivery_item",
                column: "prepared_by_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_delivery_item_case_case_id",
                table: "delivery_item");

            migrationBuilder.DropForeignKey(
                name: "FK_delivery_item_case_session_case_session_id",
                table: "delivery_item");

            migrationBuilder.DropForeignKey(
                name: "FK_delivery_item_nom_notification_delivery_group_notification_~",
                table: "delivery_item");

            migrationBuilder.DropForeignKey(
                name: "FK_delivery_item_common_law_unit_prepared_by_id",
                table: "delivery_item");

            migrationBuilder.DropIndex(
                name: "IX_delivery_item_case_id",
                table: "delivery_item");

            migrationBuilder.DropIndex(
                name: "IX_delivery_item_case_session_id",
                table: "delivery_item");

            migrationBuilder.DropIndex(
                name: "IX_delivery_item_notification_delivery_group_id",
                table: "delivery_item");

            migrationBuilder.DropIndex(
                name: "IX_delivery_item_prepared_by_id",
                table: "delivery_item");

            migrationBuilder.DropColumn(
                name: "case_id",
                table: "delivery_item");

            migrationBuilder.DropColumn(
                name: "case_session_id",
                table: "delivery_item");

            migrationBuilder.DropColumn(
                name: "notification_delivery_group_id",
                table: "delivery_item");

            migrationBuilder.DropColumn(
                name: "prepared_by_id",
                table: "delivery_item");
        }
    }
}
