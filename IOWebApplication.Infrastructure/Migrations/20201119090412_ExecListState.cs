using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ExecListState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "exec_list_state_id",
                table: "money_exec_list",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_exec_list_state",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    order_number = table.Column<int>(nullable: false),
                    code = table.Column<string>(nullable: true),
                    label = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    date_start = table.Column<DateTime>(nullable: false),
                    date_end = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_exec_list_state", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_money_exec_list_exec_list_state_id",
                table: "money_exec_list",
                column: "exec_list_state_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_exec_list_nom_exec_list_state_exec_list_state_id",
                table: "money_exec_list",
                column: "exec_list_state_id",
                principalTable: "nom_exec_list_state",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_money_exec_list_nom_exec_list_state_exec_list_state_id",
                table: "money_exec_list");

            migrationBuilder.DropTable(
                name: "nom_exec_list_state");

            migrationBuilder.DropIndex(
                name: "IX_money_exec_list_exec_list_state_id",
                table: "money_exec_list");

            migrationBuilder.DropColumn(
                name: "exec_list_state_id",
                table: "money_exec_list");
        }
    }
}
