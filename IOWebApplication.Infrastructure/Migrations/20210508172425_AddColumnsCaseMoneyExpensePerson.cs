using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCaseMoneyExpensePerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_denominator",
                table: "case_money_expense_person",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_numerator",
                table: "case_money_expense_person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_denominator",
                table: "case_money_expense_person");

            migrationBuilder.DropColumn(
                name: "amount_numerator",
                table: "case_money_expense_person");
        }
    }
}
