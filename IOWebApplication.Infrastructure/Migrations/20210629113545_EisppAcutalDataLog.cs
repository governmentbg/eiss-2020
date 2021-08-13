using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EisppAcutalDataLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "eispp_number",
                table: "eispp_event_item",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "common_eispp_actual_data_log",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    eispp_number = table.Column<string>(nullable: true),
                    court_id = table.Column<int>(nullable: false),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    error_description = table.Column<string>(nullable: true),
                    response = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_eispp_actual_data_log", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_eispp_actual_data_log");

            migrationBuilder.DropColumn(
                name: "eispp_number",
                table: "eispp_event_item");
        }
    }
}
