using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class LawUnitSignExcelListBulletin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "lawunit_sign_id",
                table: "money_exec_list",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lawunit_sign_id",
                table: "case_person_sentence_bulletin",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_money_exec_list_lawunit_sign_id",
                table: "money_exec_list",
                column: "lawunit_sign_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_sentence_bulletin_lawunit_sign_id",
                table: "case_person_sentence_bulletin",
                column: "lawunit_sign_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_common_law_unit_lawunit_sign_~",
                table: "case_person_sentence_bulletin",
                column: "lawunit_sign_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_money_exec_list_common_law_unit_lawunit_sign_id",
                table: "money_exec_list",
                column: "lawunit_sign_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_common_law_unit_lawunit_sign_~",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropForeignKey(
                name: "FK_money_exec_list_common_law_unit_lawunit_sign_id",
                table: "money_exec_list");

            migrationBuilder.DropIndex(
                name: "IX_money_exec_list_lawunit_sign_id",
                table: "money_exec_list");

            migrationBuilder.DropIndex(
                name: "IX_case_person_sentence_bulletin_lawunit_sign_id",
                table: "case_person_sentence_bulletin");

            migrationBuilder.DropColumn(
                name: "lawunit_sign_id",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "lawunit_sign_id",
                table: "case_person_sentence_bulletin");
        }
    }
}
