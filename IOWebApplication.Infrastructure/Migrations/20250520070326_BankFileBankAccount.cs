using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankFileBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_automatic",
                table: "money_payment",
                type: "boolean",
                nullable: true,
                comment: "Плащането е от автоматична обработка на банков файл");

            migrationBuilder.AddColumn<int>(
                name: "court_bank_account_id",
                table: "money_bank_file",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "По сметка");

            migrationBuilder.CreateIndex(
                name: "IX_money_bank_file_court_bank_account_id",
                table: "money_bank_file",
                column: "court_bank_account_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_bank_file_common_court_bank_account_court_bank_accoun~",
                table: "money_bank_file",
                column: "court_bank_account_id",
                principalTable: "common_court_bank_account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_bank_file_common_court_bank_account_court_bank_accoun~",
                table: "money_bank_file");

            migrationBuilder.DropIndex(
                name: "IX_money_bank_file_court_bank_account_id",
                table: "money_bank_file");

            migrationBuilder.DropColumn(
                name: "is_automatic",
                table: "money_payment");

            migrationBuilder.DropColumn(
                name: "court_bank_account_id",
                table: "money_bank_file");
        }
    }
}
