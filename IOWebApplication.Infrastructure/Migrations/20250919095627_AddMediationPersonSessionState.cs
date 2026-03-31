using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediationPersonSessionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "description_expired",
                table: "nom_mediation_point_mediator_appraisal",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "mediation_person_session_state_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "nom_mediation_person_session_state",
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
                    table.PrimaryKey("PK_nom_mediation_person_session_state", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_person_mediation_person_session_state_id",
                table: "mediation_case_person",
                column: "mediation_person_session_state_id");

            migrationBuilder.AddForeignKey(
                name: "FK_mediation_case_person_nom_mediation_person_session_state_me~",
                table: "mediation_case_person",
                column: "mediation_person_session_state_id",
                principalTable: "nom_mediation_person_session_state",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_mediation_case_person_nom_mediation_person_session_state_me~",
                table: "mediation_case_person");

            migrationBuilder.DropTable(
                name: "nom_mediation_person_session_state");

            migrationBuilder.DropIndex(
                name: "IX_mediation_case_person_mediation_person_session_state_id",
                table: "mediation_case_person");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "nom_mediation_point_mediator_appraisal");

            migrationBuilder.DropColumn(
                name: "mediation_person_session_state_id",
                table: "mediation_case_person");
        }
    }
}
