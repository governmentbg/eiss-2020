using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnAndTablesCaseMoneyExpensePerson1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_money_expense_person",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: true),
                    case_id = table.Column<int>(nullable: false),
                    case_money_expense_id = table.Column<int>(nullable: false),
                    case_person_id = table.Column<int>(nullable: false),
                    person_amount = table.Column<decimal>(nullable: false),
                    respected_amount = table.Column<decimal>(nullable: false),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_money_expense_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_money_expense_person_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_money_expense_person_case_money_expense_case_money_exp~",
                        column: x => x.case_money_expense_id,
                        principalTable: "case_money_expense",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_money_expense_person_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_money_expense_person_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_money_expense_person_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_money_expense_person_case_id",
                table: "case_money_expense_person",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_money_expense_person_case_money_expense_id",
                table: "case_money_expense_person",
                column: "case_money_expense_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_money_expense_person_case_person_id",
                table: "case_money_expense_person",
                column: "case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_money_expense_person_court_id",
                table: "case_money_expense_person",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_money_expense_person_user_id",
                table: "case_money_expense_person",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_money_expense_person");
        }
    }
}
