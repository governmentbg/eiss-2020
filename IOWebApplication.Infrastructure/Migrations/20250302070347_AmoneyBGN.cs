using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AmoneyBGN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_bgn",
                table: "money_pos_payment_result",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_bgn",
                table: "money_payment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_bgn",
                table: "money_obligation_payment",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_bgn",
                table: "money_obligation",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_bgn",
                table: "money_exec_list_obligation",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_bgn",
                table: "money_pos_payment_result");

            migrationBuilder.DropColumn(
                name: "amount_bgn",
                table: "money_payment");

            migrationBuilder.DropColumn(
                name: "amount_bgn",
                table: "money_obligation_payment");

            migrationBuilder.DropColumn(
                name: "amount_bgn",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "amount_bgn",
                table: "money_exec_list_obligation");
        }
    }
}
