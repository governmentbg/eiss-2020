using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DismissalStateForEpro : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dismissal_state_id",
                table: "case_lawunit_dismisal",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "document_id",
                table: "case_lawunit_dismisal",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "document_person_id",
                table: "case_lawunit_dismisal",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_dismissal_state",
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
                    table.PrimaryKey("PK_nom_dismissal_state", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_dismisal_dismissal_state_id",
                table: "case_lawunit_dismisal",
                column: "dismissal_state_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_dismisal_nom_dismissal_state_dismissal_state_id",
                table: "case_lawunit_dismisal",
                column: "dismissal_state_id",
                principalTable: "nom_dismissal_state",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_dismisal_nom_dismissal_state_dismissal_state_id",
                table: "case_lawunit_dismisal");

            migrationBuilder.DropTable(
                name: "nom_dismissal_state");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_dismisal_dismissal_state_id",
                table: "case_lawunit_dismisal");

            migrationBuilder.DropColumn(
                name: "dismissal_state_id",
                table: "case_lawunit_dismisal");

            migrationBuilder.DropColumn(
                name: "document_id",
                table: "case_lawunit_dismisal");

            migrationBuilder.DropColumn(
                name: "document_person_id",
                table: "case_lawunit_dismisal");
        }
    }
}
