using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class FromToDateDocumentTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_from",
                table: "document_template",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_to",
                table: "document_template",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "have_from_to_date",
                table: "common_html_template",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_from",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "date_to",
                table: "document_template");

            migrationBuilder.DropColumn(
                name: "have_from_to_date",
                table: "common_html_template");
        }
    }
}
