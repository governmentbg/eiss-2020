using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class MoneyTypeRko1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_for_rko",
                table: "nom_money_type",
                newName: "is_transport");

            migrationBuilder.AddColumn<bool>(
                name: "is_earning",
                table: "nom_money_type",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_earning",
                table: "nom_money_type");

            migrationBuilder.RenameColumn(
                name: "is_transport",
                table: "nom_money_type",
                newName: "is_for_rko");
        }
    }
}
