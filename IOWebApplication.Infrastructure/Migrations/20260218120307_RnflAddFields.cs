using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RnflAddFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "rnfl_effective_immediately",
                table: "case_session_act_h",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "rnfl_effective_immediately",
                table: "case_session_act",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rnfl_process_type_id",
                table: "case_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rnfl_process_type_id",
                table: "case",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_rnfl_process_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_rnfl_process_type", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_rnfl_process_type_id",
                table: "case",
                column: "rnfl_process_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_nom_rnfl_process_type_rnfl_process_type_id",
                table: "case",
                column: "rnfl_process_type_id",
                principalTable: "nom_rnfl_process_type",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_nom_rnfl_process_type_rnfl_process_type_id",
                table: "case");

            migrationBuilder.DropTable(
                name: "nom_rnfl_process_type");

            migrationBuilder.DropIndex(
                name: "IX_case_rnfl_process_type_id",
                table: "case");

            migrationBuilder.DropColumn(
                name: "rnfl_effective_immediately",
                table: "case_session_act_h");

            migrationBuilder.DropColumn(
                name: "rnfl_effective_immediately",
                table: "case_session_act");

            migrationBuilder.DropColumn(
                name: "rnfl_process_type_id",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "rnfl_process_type_id",
                table: "case");
        }
    }
}
