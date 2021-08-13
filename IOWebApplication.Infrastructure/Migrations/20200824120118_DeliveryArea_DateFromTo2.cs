using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DeliveryArea_DateFromTo2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTo",
                table: "delivery_area_address",
                newName: "date_to");

            migrationBuilder.RenameColumn(
                name: "DateFrom",
                table: "delivery_area_address",
                newName: "date_from");

            migrationBuilder.RenameColumn(
                name: "DateTo",
                table: "delivery_area",
                newName: "date_to");

            migrationBuilder.RenameColumn(
                name: "DateFrom",
                table: "delivery_area",
                newName: "date_from");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date_to",
                table: "delivery_area_address",
                newName: "DateTo");

            migrationBuilder.RenameColumn(
                name: "date_from",
                table: "delivery_area_address",
                newName: "DateFrom");

            migrationBuilder.RenameColumn(
                name: "date_to",
                table: "delivery_area",
                newName: "DateTo");

            migrationBuilder.RenameColumn(
                name: "date_from",
                table: "delivery_area",
                newName: "DateFrom");
        }
    }
}
