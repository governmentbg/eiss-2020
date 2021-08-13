using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EisppSignal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_eispp_signal",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    date_create = table.Column<DateTime>(nullable: false),
                    case_id = table.Column<int>(nullable: true),
                    response_data = table.Column<string>(nullable: true),
                    structure_id = table.Column<int>(nullable: true),
                    eispp_number = table.Column<string>(nullable: true),
                    case_type = table.Column<int>(nullable: true),
                    exact_case_type = table.Column<int>(nullable: true),
                    case_year = table.Column<int>(nullable: true),
                    short_number = table.Column<int>(nullable: true),
                    message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_eispp_signal", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_eispp_signal_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_eispp_signal_case_id",
                table: "common_eispp_signal",
                column: "case_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_eispp_signal");
        }
    }
}
