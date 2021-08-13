using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnCourtDepartmentCaseBankAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_instance_id",
                table: "common_court_department",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lawunit_id",
                table: "case_bank_account",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_court_department_case_instance_id",
                table: "common_court_department",
                column: "case_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_bank_account_lawunit_id",
                table: "case_bank_account",
                column: "lawunit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_bank_account_common_law_unit_lawunit_id",
                table: "case_bank_account",
                column: "lawunit_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_common_court_department_nom_case_instance_case_instance_id",
                table: "common_court_department",
                column: "case_instance_id",
                principalTable: "nom_case_instance",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_bank_account_common_law_unit_lawunit_id",
                table: "case_bank_account");

            migrationBuilder.DropForeignKey(
                name: "FK_common_court_department_nom_case_instance_case_instance_id",
                table: "common_court_department");

            migrationBuilder.DropIndex(
                name: "IX_common_court_department_case_instance_id",
                table: "common_court_department");

            migrationBuilder.DropIndex(
                name: "IX_case_bank_account_lawunit_id",
                table: "case_bank_account");

            migrationBuilder.DropColumn(
                name: "case_instance_id",
                table: "common_court_department");

            migrationBuilder.DropColumn(
                name: "lawunit_id",
                table: "case_bank_account");
        }
    }
}
