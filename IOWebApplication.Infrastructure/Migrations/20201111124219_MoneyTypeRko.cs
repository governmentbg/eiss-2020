using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class MoneyTypeRko : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NoMoney",
                table: "nom_money_type",
                newName: "no_money");

            migrationBuilder.AddColumn<bool>(
                name: "is_for_rko",
                table: "nom_money_type",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_for_rko",
                table: "nom_money_type");

            migrationBuilder.RenameColumn(
                name: "no_money",
                table: "nom_money_type",
                newName: "NoMoney");
        }
    }
}
