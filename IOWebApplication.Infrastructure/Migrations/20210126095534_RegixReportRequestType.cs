using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class RegixReportRequestType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "regix_request_type_id",
                table: "regix_report",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_regix_report_regix_request_type_id",
                table: "regix_report",
                column: "regix_request_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_regix_report_nom_regix_request_type_regix_request_type_id",
                table: "regix_report",
                column: "regix_request_type_id",
                principalTable: "nom_regix_request_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_regix_report_nom_regix_request_type_regix_request_type_id",
                table: "regix_report");

            migrationBuilder.DropIndex(
                name: "IX_regix_report_regix_request_type_id",
                table: "regix_report");

            migrationBuilder.DropColumn(
                name: "regix_request_type_id",
                table: "regix_report");
        }
    }
}
