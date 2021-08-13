using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class FromToCaseSessionIdInLawUnits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_lawunit_change",
                table: "case_session_h",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_lawunit_change",
                table: "case_session",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "from_case_session_id",
                table: "case_lawunit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "to_case_session_id",
                table: "case_lawunit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_from_case_session_id",
                table: "case_lawunit",
                column: "from_case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_to_case_session_id",
                table: "case_lawunit",
                column: "to_case_session_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_case_session_from_case_session_id",
                table: "case_lawunit",
                column: "from_case_session_id",
                principalTable: "case_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_case_session_to_case_session_id",
                table: "case_lawunit",
                column: "to_case_session_id",
                principalTable: "case_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_case_session_from_case_session_id",
                table: "case_lawunit");

            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_case_session_to_case_session_id",
                table: "case_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_from_case_session_id",
                table: "case_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_to_case_session_id",
                table: "case_lawunit");

            migrationBuilder.DropColumn(
                name: "case_lawunit_change",
                table: "case_session_h");

            migrationBuilder.DropColumn(
                name: "case_lawunit_change",
                table: "case_session");

            migrationBuilder.DropColumn(
                name: "from_case_session_id",
                table: "case_lawunit");

            migrationBuilder.DropColumn(
                name: "to_case_session_id",
                table: "case_lawunit");
        }
    }
}
