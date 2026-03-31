using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediationProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_result_act~",
                table: "case_session_act_selection_protocol_acts");

            migrationBuilder.AddColumn<int>(
                name: "mediation_procedure_id",
                table: "case_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mediation_procedure_id",
                table: "case",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_mediation_procedure",
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
                    table.PrimaryKey("PK_nom_mediation_procedure", x => x.id);
                },
                comment: "Видове процедури по медиация");

            migrationBuilder.CreateIndex(
                name: "IX_case_mediation_procedure_id",
                table: "case",
                column: "mediation_procedure_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_nom_mediation_procedure_mediation_procedure_id",
                table: "case",
                column: "mediation_procedure_id",
                principalTable: "nom_mediation_procedure",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_complain_r~",
                table: "case_session_act_selection_protocol_acts",
                column: "act_result_id",
                principalTable: "nom_act_complain_result",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_nom_mediation_procedure_mediation_procedure_id",
                table: "case");

            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_complain_r~",
                table: "case_session_act_selection_protocol_acts");

            migrationBuilder.DropTable(
                name: "nom_mediation_procedure");

            migrationBuilder.DropIndex(
                name: "IX_case_mediation_procedure_id",
                table: "case");

            migrationBuilder.DropColumn(
                name: "mediation_procedure_id",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "mediation_procedure_id",
                table: "case");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_result_act~",
                table: "case_session_act_selection_protocol_acts",
                column: "act_result_id",
                principalTable: "nom_act_result",
                principalColumn: "id");
        }
    }
}
