using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankFilePaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_bank_file_payment_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_bank_file_payment_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_bank_file_payment_type_code",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_bank_file_payment_type_code", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_bank_code_payment_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор банка"),
                    payment_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор начин на плащане"),
                    bank_codes = table.Column<string[]>(type: "jsonb", nullable: true, comment: "Банковите кодове към начин на плащане при нас - ще се ползва за файл MT940")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_bank_code_payment_type", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_bank_code_payment_type_nom_bank_bank_id",
                        column: x => x.bank_id,
                        principalTable: "nom_bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_bank_code_payment_type_nom_bank_file_payment_type_payme~",
                        column: x => x.payment_type_id,
                        principalTable: "nom_bank_file_payment_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_money_bank_file_payment_payment_type_code_id",
                table: "money_bank_file_payment",
                column: "payment_type_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_money_bank_file_payment_payment_type_id",
                table: "money_bank_file_payment",
                column: "payment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_bank_code_payment_type_bank_id",
                table: "nom_bank_code_payment_type",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_bank_code_payment_type_payment_type_id",
                table: "nom_bank_code_payment_type",
                column: "payment_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_bank_file_payment_nom_bank_file_payment_type_code_pay~",
                table: "money_bank_file_payment",
                column: "payment_type_code_id",
                principalTable: "nom_bank_file_payment_type_code",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_money_bank_file_payment_nom_bank_file_payment_type_payment_~",
                table: "money_bank_file_payment",
                column: "payment_type_id",
                principalTable: "nom_bank_file_payment_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_bank_file_payment_nom_bank_file_payment_type_code_pay~",
                table: "money_bank_file_payment");

            migrationBuilder.DropForeignKey(
                name: "FK_money_bank_file_payment_nom_bank_file_payment_type_payment_~",
                table: "money_bank_file_payment");

            migrationBuilder.DropTable(
                name: "nom_bank_code_payment_type");

            migrationBuilder.DropTable(
                name: "nom_bank_file_payment_type_code");

            migrationBuilder.DropTable(
                name: "nom_bank_file_payment_type");

            migrationBuilder.DropIndex(
                name: "IX_money_bank_file_payment_payment_type_code_id",
                table: "money_bank_file_payment");

            migrationBuilder.DropIndex(
                name: "IX_money_bank_file_payment_payment_type_id",
                table: "money_bank_file_payment");
        }
    }
}
