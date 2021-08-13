using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnCaseBankAccountCaseFastProcess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VisibleOrder",
                table: "case_fast_process",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VisibleEL",
                table: "case_bank_account",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisibleOrder",
                table: "case_fast_process");

            migrationBuilder.DropColumn(
                name: "VisibleEL",
                table: "case_bank_account");
        }
    }
}
