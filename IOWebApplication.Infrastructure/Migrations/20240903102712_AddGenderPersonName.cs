using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderPersonName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "money_obligation_receive",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "money_obligation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "electronic_document_person",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "document_person",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "common_person",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "common_law_unit_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "common_law_unit",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "common_institution",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "case_person_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                table: "case_person",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "money_obligation_receive");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "electronic_document_person");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "document_person");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "common_person");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "common_law_unit_h");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "common_law_unit");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "common_institution");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "gender_id",
                table: "case_person");
        }
    }
}
