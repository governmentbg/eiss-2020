using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExpenseOrderJudge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "lawunit_sign_id",
                table: "money_expense_order",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_money_expense_order_lawunit_sign_id",
                table: "money_expense_order",
                column: "lawunit_sign_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_expense_order_common_law_unit_lawunit_sign_id",
                table: "money_expense_order",
                column: "lawunit_sign_id",
                principalTable: "common_law_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_expense_order_common_law_unit_lawunit_sign_id",
                table: "money_expense_order");

            migrationBuilder.DropIndex(
                name: "IX_money_expense_order_lawunit_sign_id",
                table: "money_expense_order");

            migrationBuilder.DropColumn(
                name: "lawunit_sign_id",
                table: "money_expense_order");
        }
    }
}
