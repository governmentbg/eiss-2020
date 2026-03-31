using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentCasePersonGid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "person_gid",
                table: "document_person",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "represents_gid",
                table: "document_person",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "person_gid",
                table: "case_person_h",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "represents_gid",
                table: "case_person_h",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "person_gid",
                table: "case_person",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "represents_gid",
                table: "case_person",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "person_gid",
                table: "document_person");

            migrationBuilder.DropColumn(
                name: "represents_gid",
                table: "document_person");

            migrationBuilder.DropColumn(
                name: "person_gid",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "represents_gid",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "person_gid",
                table: "case_person");

            migrationBuilder.DropColumn(
                name: "represents_gid",
                table: "case_person");
        }
    }
}
