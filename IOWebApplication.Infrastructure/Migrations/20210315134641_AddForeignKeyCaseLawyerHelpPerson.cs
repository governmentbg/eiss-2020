using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddForeignKeyCaseLawyerHelpPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_person_specified_lawyer_lawunit_id",
                table: "case_lawyer_help_person",
                column: "specified_lawyer_lawunit_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_person_common_law_unit_specified_lawyer_la~",
                table: "case_lawyer_help_person",
                column: "specified_lawyer_lawunit_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_person_common_law_unit_specified_lawyer_la~",
                table: "case_lawyer_help_person");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_person_specified_lawyer_lawunit_id",
                table: "case_lawyer_help_person");
        }
    }
}
