using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FastProcessSelectionCourtKoef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "bazov_koef_court",
                table: "common_fast_process_selection_court",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "bazov_koef_court_judge",
                table: "common_fast_process_selection_court",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "bazov_koef_sreden_all_court_judge",
                table: "common_fast_process_selection_court",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bazov_koef_court",
                table: "common_fast_process_selection_court");

            migrationBuilder.DropColumn(
                name: "bazov_koef_court_judge",
                table: "common_fast_process_selection_court");

            migrationBuilder.DropColumn(
                name: "bazov_koef_sreden_all_court_judge",
                table: "common_fast_process_selection_court");
        }
    }
}
