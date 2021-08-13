using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsAndTableFor410417 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "respected_amount",
                table: "case_money_collection_person",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "label",
                table: "case_money_collection",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_collection_end_date_type_id",
                table: "case_money_collection",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motive",
                table: "case_money_collection",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_money_collection_end_date_type",
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
                    table.PrimaryKey("PK_nom_money_collection_end_date_type", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_money_collection_money_collection_end_date_type_id",
                table: "case_money_collection",
                column: "money_collection_end_date_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_money_collection_nom_money_collection_end_date_type_mo~",
                table: "case_money_collection",
                column: "money_collection_end_date_type_id",
                principalTable: "nom_money_collection_end_date_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_money_collection_nom_money_collection_end_date_type_mo~",
                table: "case_money_collection");

            migrationBuilder.DropTable(
                name: "nom_money_collection_end_date_type");

            migrationBuilder.DropIndex(
                name: "IX_case_money_collection_money_collection_end_date_type_id",
                table: "case_money_collection");

            migrationBuilder.DropColumn(
                name: "respected_amount",
                table: "case_money_collection_person");

            migrationBuilder.DropColumn(
                name: "label",
                table: "case_money_collection");

            migrationBuilder.DropColumn(
                name: "money_collection_end_date_type_id",
                table: "case_money_collection");

            migrationBuilder.DropColumn(
                name: "motive",
                table: "case_money_collection");
        }
    }
}
