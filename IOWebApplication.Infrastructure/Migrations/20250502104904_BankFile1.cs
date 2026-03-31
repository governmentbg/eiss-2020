using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankFile1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_bank_file_common_court_court_id",
                table: "money_bank_file");

            migrationBuilder.DropColumn(
                name: "description",
                table: "money_bank_file");

            migrationBuilder.DropColumn(
                name: "file_status",
                table: "money_bank_file");

            migrationBuilder.AddColumn<string>(
                name: "line_text",
                table: "money_bank_file_payment",
                type: "text",
                nullable: true,
                comment: "Целият ред както е във файла");

            migrationBuilder.AddColumn<string>(
                name: "payment_type_code",
                table: "money_bank_file_payment",
                type: "text",
                nullable: true,
                comment: "К - кредит, L - сторно кредит, D - дебит, E - сторно дебит");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "money_bank_file",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Съд",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Съд");

            migrationBuilder.AddForeignKey(
                name: "FK_money_bank_file_common_court_court_id",
                table: "money_bank_file",
                column: "court_id",
                principalTable: "common_court",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_bank_file_common_court_court_id",
                table: "money_bank_file");

            migrationBuilder.DropColumn(
                name: "line_text",
                table: "money_bank_file_payment");

            migrationBuilder.DropColumn(
                name: "payment_type_code",
                table: "money_bank_file_payment");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "money_bank_file",
                type: "integer",
                nullable: true,
                comment: "Съд",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Съд");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "money_bank_file",
                type: "text",
                nullable: true,
                comment: "Описание");

            migrationBuilder.AddColumn<int>(
                name: "file_status",
                table: "money_bank_file",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Статус на файла 1 - Необработен, 2 - Обработен");

            migrationBuilder.AddForeignKey(
                name: "FK_money_bank_file_common_court_court_id",
                table: "money_bank_file",
                column: "court_id",
                principalTable: "common_court",
                principalColumn: "id");
        }
    }
}
