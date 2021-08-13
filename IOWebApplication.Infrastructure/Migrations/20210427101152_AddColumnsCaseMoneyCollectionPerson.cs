using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCaseMoneyCollectionPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount_denominator",
                table: "case_money_collection_person",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amount_numerator",
                table: "case_money_collection_person",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount_denominator",
                table: "case_money_collection_person");

            migrationBuilder.DropColumn(
                name: "amount_numerator",
                table: "case_money_collection_person");
        }
    }
}
