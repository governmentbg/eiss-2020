using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "money_bank_file",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_name = table.Column<string>(type: "text", nullable: true, comment: "Име на файла"),
                    iban = table.Column<string>(type: "text", nullable: true, comment: "IBAN"),
                    court_id = table.Column<int>(type: "integer", nullable: true, comment: "Съд"),
                    file_status = table.Column<int>(type: "integer", nullable: false, comment: "Статус на файла 1 - Необработен, 2 - Обработен"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Описание"),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_money_bank_file", x => x.id);
                    table.ForeignKey(
                        name: "FK_money_bank_file_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                },
                comment: "Банкови файлове с извлечения по сметка");

            migrationBuilder.CreateTable(
                name: "money_bank_file_payment",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_file_id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор на банковия файл"),
                    bank_id = table.Column<string>(type: "text", nullable: true, comment: "Банков идентификатор"),
                    paid_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на плащане"),
                    amount = table.Column<decimal>(type: "numeric", nullable: false, comment: "Сума"),
                    currency_id = table.Column<int>(type: "integer", nullable: false, comment: "Валута"),
                    payment_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Начин на плащане - ПОС, Платежно"),
                    debtor_names = table.Column<string>(type: "text", nullable: true, comment: "Име на задълженото лице"),
                    debtor_identifier = table.Column<string>(type: "text", nullable: true, comment: "Идентификатор на задълженото лице"),
                    sender_name = table.Column<string>(type: "text", nullable: true, comment: "Вносител"),
                    payment_Info = table.Column<string>(type: "text", nullable: true, comment: "Основание"),
                    payment_description = table.Column<string>(type: "text", nullable: true, comment: "Основание2"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Описание от обработката"),
                    payment_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на плащане"),
                    payment_status = table.Column<int>(type: "integer", nullable: false, comment: "Статус от обработката")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_money_bank_file_payment", x => x.id);
                    table.ForeignKey(
                        name: "FK_money_bank_file_payment_money_bank_file_bank_file_id",
                        column: x => x.bank_file_id,
                        principalTable: "money_bank_file",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_money_bank_file_payment_money_payment_payment_id",
                        column: x => x.payment_id,
                        principalTable: "money_payment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_money_bank_file_payment_nom_currency_currency_id",
                        column: x => x.currency_id,
                        principalTable: "nom_currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Плащанията в един банков файл");

            migrationBuilder.CreateIndex(
                name: "IX_money_bank_file_court_id",
                table: "money_bank_file",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_money_bank_file_payment_bank_file_id",
                table: "money_bank_file_payment",
                column: "bank_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_money_bank_file_payment_currency_id",
                table: "money_bank_file_payment",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_money_bank_file_payment_payment_id",
                table: "money_bank_file_payment",
                column: "payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "money_bank_file_payment");

            migrationBuilder.DropTable(
                name: "money_bank_file");
        }
    }
}
