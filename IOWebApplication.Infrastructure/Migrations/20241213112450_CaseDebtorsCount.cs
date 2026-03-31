using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseDebtorsCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "debtors_count",
                table: "case_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "debtors_count",
                table: "case",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "debtors_count",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "debtors_count",
                table: "case");
        }
    }
}
