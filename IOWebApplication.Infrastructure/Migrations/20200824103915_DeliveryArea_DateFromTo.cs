using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DeliveryArea_DateFromTo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateFrom",
                table: "delivery_area_address",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTo",
                table: "delivery_area_address",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFrom",
                table: "delivery_area",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTo",
                table: "delivery_area",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFrom",
                table: "delivery_area_address");

            migrationBuilder.DropColumn(
                name: "DateTo",
                table: "delivery_area_address");

            migrationBuilder.DropColumn(
                name: "DateFrom",
                table: "delivery_area");

            migrationBuilder.DropColumn(
                name: "DateTo",
                table: "delivery_area");
        }
    }
}
