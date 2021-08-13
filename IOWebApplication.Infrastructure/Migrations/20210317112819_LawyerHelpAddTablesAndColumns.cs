using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class LawyerHelpAddTablesAndColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "date_expired",
                table: "case_lawyer_help_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description_expired",
                table: "case_lawyer_help_person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_expired_id",
                table: "case_lawyer_help_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "act_appointment_id",
                table: "case_lawyer_help",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lawyer_help_basis_appointment_id",
                table: "case_lawyer_help",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_lawyer_help_basis_appointment",
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
                    table.PrimaryKey("PK_nom_lawyer_help_basis_appointment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_lawyer_help_type_case_group",
                columns: table => new
                {
                    lawyer_help_type_id = table.Column<int>(nullable: false),
                    case_group_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_lawyer_help_type_case_group", x => new { x.lawyer_help_type_id, x.case_group_id });
                    table.ForeignKey(
                        name: "FK_nom_lawyer_help_type_case_group_nom_case_group_case_group_id",
                        column: x => x.case_group_id,
                        principalTable: "nom_case_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_lawyer_help_type_case_group_nom_lawyer_help_type_lawyer~",
                        column: x => x.lawyer_help_type_id,
                        principalTable: "nom_lawyer_help_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_person_user_expired_id",
                table: "case_lawyer_help_person",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_act_appointment_id",
                table: "case_lawyer_help",
                column: "act_appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_lawyer_help_basis_appointment_id",
                table: "case_lawyer_help",
                column: "lawyer_help_basis_appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_lawyer_help_type_case_group_case_group_id",
                table: "nom_lawyer_help_type_case_group",
                column: "case_group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_case_session_act_act_appointment_id",
                table: "case_lawyer_help",
                column: "act_appointment_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_nom_lawyer_help_basis_appointment_lawyer_h~",
                table: "case_lawyer_help",
                column: "lawyer_help_basis_appointment_id",
                principalTable: "nom_lawyer_help_basis_appointment",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_person_identity_users_user_expired_id",
                table: "case_lawyer_help_person",
                column: "user_expired_id",
                principalTable: "identity_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_case_session_act_act_appointment_id",
                table: "case_lawyer_help");

            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_nom_lawyer_help_basis_appointment_lawyer_h~",
                table: "case_lawyer_help");

            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_person_identity_users_user_expired_id",
                table: "case_lawyer_help_person");

            migrationBuilder.DropTable(
                name: "nom_lawyer_help_basis_appointment");

            migrationBuilder.DropTable(
                name: "nom_lawyer_help_type_case_group");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_person_user_expired_id",
                table: "case_lawyer_help_person");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_act_appointment_id",
                table: "case_lawyer_help");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_lawyer_help_basis_appointment_id",
                table: "case_lawyer_help");

            migrationBuilder.DropColumn(
                name: "date_expired",
                table: "case_lawyer_help_person");

            migrationBuilder.DropColumn(
                name: "description_expired",
                table: "case_lawyer_help_person");

            migrationBuilder.DropColumn(
                name: "user_expired_id",
                table: "case_lawyer_help_person");

            migrationBuilder.DropColumn(
                name: "act_appointment_id",
                table: "case_lawyer_help");

            migrationBuilder.DropColumn(
                name: "lawyer_help_basis_appointment_id",
                table: "case_lawyer_help");
        }
    }
}
