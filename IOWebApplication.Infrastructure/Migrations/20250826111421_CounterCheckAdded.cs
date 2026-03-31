using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CounterCheckAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_counter_check",
                columns: table => new
                {
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    counter_value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date_check = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_counter_check", x => new { x.source_type, x.counter_value });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_counter_check");
        }
    }
}
