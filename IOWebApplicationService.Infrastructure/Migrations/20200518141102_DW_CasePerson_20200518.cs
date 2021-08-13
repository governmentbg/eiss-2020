using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_CasePerson_20200518 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "department_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "family_2_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "family_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "latin_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "middle_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "person_source_id",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "person_source_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "person_source_type",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "person_source_type_name",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uic",
                table: "dw_case_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "uic_type_id",
                table: "dw_case_person",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "uic_type_name",
                table: "dw_case_person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "department_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "family_2_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "family_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "full_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "latin_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "middle_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "person_source_id",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "person_source_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "person_source_type",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "person_source_type_name",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "uic",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "uic_type_id",
                table: "dw_case_person");

            migrationBuilder.DropColumn(
                name: "uic_type_name",
                table: "dw_case_person");
        }
    }
}
