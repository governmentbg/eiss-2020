using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankFilePaymentExpire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "money_bank_file_payment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "money_bank_file_payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "money_bank_file_payment",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "money_bank_file_payment");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "money_bank_file_payment");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "money_bank_file_payment");
        }
    }
}
