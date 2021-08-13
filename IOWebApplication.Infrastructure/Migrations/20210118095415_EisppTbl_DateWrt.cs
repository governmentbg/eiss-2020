using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EisppTbl_DateWrt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_wrt",
                table: "nom_eispp_tbl_element",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_wrt",
                table: "nom_eispp_tbl",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_wrt",
                table: "nom_eispp_tbl_element");

            migrationBuilder.DropColumn(
                name: "date_wrt",
                table: "nom_eispp_tbl");
        }
    }
}
