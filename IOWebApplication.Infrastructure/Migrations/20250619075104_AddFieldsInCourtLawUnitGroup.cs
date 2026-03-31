using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsInCourtLawUnitGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "court_department_id",
                table: "common_court_lawunit_group",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_to_description",
                table: "common_court_lawunit_group",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_group_court_department_id",
                table: "common_court_lawunit_group",
                column: "court_department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_court_lawunit_group_common_court_department_court_de~",
                table: "common_court_lawunit_group",
                column: "court_department_id",
                principalTable: "common_court_department",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_court_lawunit_group_common_court_department_court_de~",
                table: "common_court_lawunit_group");

            migrationBuilder.DropIndex(
                name: "IX_common_court_lawunit_group_court_department_id",
                table: "common_court_lawunit_group");

            migrationBuilder.DropColumn(
                name: "court_department_id",
                table: "common_court_lawunit_group");

            migrationBuilder.DropColumn(
                name: "date_to_description",
                table: "common_court_lawunit_group");
        }
    }
}
