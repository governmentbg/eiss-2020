using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddCoumnCaseLawyerHelpPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "specified_lawyer",
                table: "case_lawyer_help_person");

            migrationBuilder.AddColumn<int>(
                name: "specified_lawyer_lawunit_id",
                table: "case_lawyer_help_person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "specified_lawyer_lawunit_id",
                table: "case_lawyer_help_person");

            migrationBuilder.AddColumn<string>(
                name: "specified_lawyer",
                table: "case_lawyer_help_person",
                nullable: true);
        }
    }
}
