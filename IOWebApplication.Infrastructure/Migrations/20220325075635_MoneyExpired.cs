using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class MoneyExpired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "money_expense_order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "money_expense_order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "money_expense_order",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "money_exec_list",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "money_exec_list",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "money_exec_list",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "money_exchange_doc",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "money_exchange_doc",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "money_exchange_doc",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "money_expense_order");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "money_expense_order");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "money_expense_order");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "money_exchange_doc");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "money_exchange_doc");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "money_exchange_doc");
        }
    }
}
