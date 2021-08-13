using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentNotification_Lawunit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_document_notification_lawunit_id",
                table: "document_notification",
                column: "lawunit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_notification_common_law_unit_lawunit_id",
                table: "document_notification",
                column: "lawunit_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_notification_common_law_unit_lawunit_id",
                table: "document_notification");

            migrationBuilder.DropIndex(
                name: "IX_document_notification_lawunit_id",
                table: "document_notification");
        }
    }
}
