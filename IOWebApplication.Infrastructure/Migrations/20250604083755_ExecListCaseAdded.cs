using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExecListCaseAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_id",
                table: "money_exec_list",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_money_exec_list_case_id",
                table: "money_exec_list",
                column: "case_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_exec_list_case_case_id",
                table: "money_exec_list",
                column: "case_id",
                principalTable: "case",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_exec_list_case_case_id",
                table: "money_exec_list");

            migrationBuilder.DropIndex(
                name: "IX_money_exec_list_case_id",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "case_id",
                table: "money_exec_list");
        }
    }
}
