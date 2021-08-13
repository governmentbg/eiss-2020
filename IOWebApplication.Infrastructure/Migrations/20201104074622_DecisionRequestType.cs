using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DecisionRequestType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "decision_request_type_id",
                table: "document_decision_case",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_decision_request_type",
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
                    table.PrimaryKey("PK_nom_decision_request_type", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_decision_case_decision_request_type_id",
                table: "document_decision_case",
                column: "decision_request_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_decision_case_nom_decision_request_type_decision_r~",
                table: "document_decision_case",
                column: "decision_request_type_id",
                principalTable: "nom_decision_request_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_decision_case_nom_decision_request_type_decision_r~",
                table: "document_decision_case");

            migrationBuilder.DropTable(
                name: "nom_decision_request_type");

            migrationBuilder.DropIndex(
                name: "IX_document_decision_case_decision_request_type_id",
                table: "document_decision_case");

            migrationBuilder.DropColumn(
                name: "decision_request_type_id",
                table: "document_decision_case");
        }
    }
}
