using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JudgeCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "judge_count_id",
                table: "common_fast_process_selection_court",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "judge_count",
                table: "common_court",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "judge_count_id",
                table: "common_fast_process_selection_court");

            migrationBuilder.DropColumn(
                name: "judge_count",
                table: "common_court");
        }
    }
}
