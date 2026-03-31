using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediationTypeChoiceMediator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mediation_type_choice_mediator_id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на вид избор на медиатора в делото");

            migrationBuilder.CreateTable(
                name: "nom_mediation_type_choice_mediator",
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
                    table.PrimaryKey("PK_nom_mediation_type_choice_mediator", x => x.id);
                },
                comment: "Вид избор на медиатор в дело");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_mediation_type_choice_mediator_id",
                table: "mediation_case_mediator",
                column: "mediation_type_choice_mediator_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_mediator_nom_mediation_type_choice_mediator_~",
                table: "mediation_case_mediator",
                column: "mediation_type_choice_mediator_id",
                principalTable: "nom_mediation_type_choice_mediator",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_mediator_nom_mediation_type_choice_mediator_~",
                table: "mediation_case_mediator");

            migrationBuilder.DropTable(
                name: "nom_mediation_type_choice_mediator");

            migrationBuilder.DropIndex(
                name: "IX_mediation_case_mediator_mediation_type_choice_mediator_id",
                table: "mediation_case_mediator");

            migrationBuilder.DropColumn(
                name: "mediation_type_choice_mediator_id",
                table: "mediation_case_mediator");
        }
    }
}
