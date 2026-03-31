using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IspnKindAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ispn_kind",
                table: "case_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ispn_kind",
                table: "case",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ispn_kind",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "ispn_kind",
                table: "case");
        }
    }
}
