using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankFile2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_type_code",
                table: "money_bank_file_payment");

            migrationBuilder.AddColumn<int>(
                name: "payment_type_code_id",
                table: "money_bank_file_payment",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "кредит/дебит");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_type_code_id",
                table: "money_bank_file_payment");

            migrationBuilder.AddColumn<string>(
                name: "payment_type_code",
                table: "money_bank_file_payment",
                type: "text",
                nullable: true,
                comment: "К - кредит, L - сторно кредит, D - дебит, E - сторно дебит");
        }
    }
}
