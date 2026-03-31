using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedActAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "vpos_paid_in_court_id",
                table: "electronic_document",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "corrected_act_id",
                table: "case_session_act_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "corrected_act_id",
                table: "case_session_act",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_vpos_paid_in_court_id",
                table: "electronic_document",
                column: "vpos_paid_in_court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_corrected_act_id",
                table: "case_session_act",
                column: "corrected_act_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_case_session_act_corrected_act_id",
                table: "case_session_act",
                column: "corrected_act_id",
                principalTable: "case_session_act",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_electronic_document_common_court_vpos_paid_in_court_id",
                table: "electronic_document",
                column: "vpos_paid_in_court_id",
                principalTable: "common_court",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_case_session_act_corrected_act_id",
                table: "case_session_act");

            migrationBuilder.DropForeignKey(
                name: "FK_electronic_document_common_court_vpos_paid_in_court_id",
                table: "electronic_document");

            migrationBuilder.DropIndex(
                name: "IX_electronic_document_vpos_paid_in_court_id",
                table: "electronic_document");

            migrationBuilder.DropIndex(
                name: "IX_case_session_act_corrected_act_id",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "vpos_paid_in_court_id",
                table: "electronic_document");

            migrationBuilder.DropColumn(
                name: "corrected_act_id",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "corrected_act_id",
                table: "case_session_act");
        }
    }
}
