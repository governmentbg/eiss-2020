using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnCaseBankAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_bank_account_common_law_unit_lawunit_id",
                table: "case_bank_account");

            migrationBuilder.RenameColumn(
                name: "lawunit_id",
                table: "case_bank_account",
                newName: "case_person_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_bank_account_lawunit_id",
                table: "case_bank_account",
                newName: "IX_case_bank_account_case_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_bank_account_case_person_case_person_id",
                table: "case_bank_account",
                column: "case_person_id",
                principalTable: "case_person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_bank_account_case_person_case_person_id",
                table: "case_bank_account");

            migrationBuilder.RenameColumn(
                name: "case_person_id",
                table: "case_bank_account",
                newName: "lawunit_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_bank_account_case_person_id",
                table: "case_bank_account",
                newName: "IX_case_bank_account_lawunit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_bank_account_common_law_unit_lawunit_id",
                table: "case_bank_account",
                column: "lawunit_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
