using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EesppTablesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "eespp_assigned_lawyer_id",
                table: "case_lawyer_help_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "eespp_person_state_id",
                table: "case_lawyer_help_person",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_eespp_lawyer_state",
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
                    table.PrimaryKey("PK_nom_eespp_lawyer_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_eespp_person_state",
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
                    table.PrimaryKey("PK_nom_eespp_person_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "case_lawyer_help_assigned_lawyer",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    case_lawyer_help_id = table.Column<int>(nullable: false),
                    DateReturned = table.Column<DateTime>(nullable: false),
                    lawyer_number = table.Column<string>(nullable: true),
                    lawyer_name = table.Column<string>(nullable: true),
                    notification_id = table.Column<string>(nullable: true),
                    lawyer_id = table.Column<int>(nullable: false),
                    state_remark = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_lawyer_help_assigned_lawyer", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_assigned_lawyer_case_lawyer_help_case_lawy~",
                        column: x => x.case_lawyer_help_id,
                        principalTable: "case_lawyer_help",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_assigned_lawyer_nom_eespp_lawyer_state_law~",
                        column: x => x.lawyer_id,
                        principalTable: "nom_eespp_lawyer_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_person_eespp_assigned_lawyer_id",
                table: "case_lawyer_help_person",
                column: "eespp_assigned_lawyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_person_eespp_person_state_id",
                table: "case_lawyer_help_person",
                column: "eespp_person_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_case_lawyer_help_id",
                table: "case_lawyer_help_assigned_lawyer",
                column: "case_lawyer_help_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_lawyer_id",
                table: "case_lawyer_help_assigned_lawyer",
                column: "lawyer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_person_case_lawyer_help_assigned_lawyer_ee~",
                table: "case_lawyer_help_person",
                column: "eespp_assigned_lawyer_id",
                principalTable: "case_lawyer_help_assigned_lawyer",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_person_nom_eespp_person_state_eespp_person~",
                table: "case_lawyer_help_person",
                column: "eespp_person_state_id",
                principalTable: "nom_eespp_person_state",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_person_case_lawyer_help_assigned_lawyer_ee~",
                table: "case_lawyer_help_person");

            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_person_nom_eespp_person_state_eespp_person~",
                table: "case_lawyer_help_person");

            migrationBuilder.DropTable(
                name: "case_lawyer_help_assigned_lawyer");

            migrationBuilder.DropTable(
                name: "nom_eespp_person_state");

            migrationBuilder.DropTable(
                name: "nom_eespp_lawyer_state");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_person_eespp_assigned_lawyer_id",
                table: "case_lawyer_help_person");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_person_eespp_person_state_id",
                table: "case_lawyer_help_person");

            migrationBuilder.DropColumn(
                name: "eespp_assigned_lawyer_id",
                table: "case_lawyer_help_person");

            migrationBuilder.DropColumn(
                name: "eespp_person_state_id",
                table: "case_lawyer_help_person");
        }
    }
}
