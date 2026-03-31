using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankPaymentOldAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "old_amount",
                table: "money_bank_file_payment",
                type: "numeric",
                nullable: true,
                comment: "Сума");

            migrationBuilder.AddColumn<int>(
                name: "old_currency_id",
                table: "money_bank_file_payment",
                type: "integer",
                nullable: true,
                comment: "Валута");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "old_amount",
                table: "money_bank_file_payment");

            migrationBuilder.DropColumn(
                name: "old_currency_id",
                table: "money_bank_file_payment");
        }
    }
}
