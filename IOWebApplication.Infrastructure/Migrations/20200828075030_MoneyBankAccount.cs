using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class MoneyBankAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_payment_common_court_bank_account_court_bank_account_~",
                table: "money_payment");

            migrationBuilder.AlterColumn<int>(
                name: "court_bank_account_id",
                table: "money_payment",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_money_payment_common_court_bank_account_court_bank_account_~",
                table: "money_payment",
                column: "court_bank_account_id",
                principalTable: "common_court_bank_account",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_payment_common_court_bank_account_court_bank_account_~",
                table: "money_payment");

            migrationBuilder.AlterColumn<int>(
                name: "court_bank_account_id",
                table: "money_payment",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_money_payment_common_court_bank_account_court_bank_account_~",
                table: "money_payment",
                column: "court_bank_account_id",
                principalTable: "common_court_bank_account",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
