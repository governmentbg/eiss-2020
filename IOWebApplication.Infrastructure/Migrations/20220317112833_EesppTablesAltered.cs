using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class EesppTablesAltered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_session_act_assigned_id",
                table: "case_lawyer_help_assigned_lawyer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "case_lawyer_help_assigned_lawyer_person",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    case_lawyer_help_assigned_lawyer_id = table.Column<int>(nullable: false),
                    case_lawyer_help_person_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_lawyer_help_assigned_lawyer_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_assigned_lawyer_person_case_lawyer_help_as~",
                        column: x => x.case_lawyer_help_assigned_lawyer_id,
                        principalTable: "case_lawyer_help_assigned_lawyer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawyer_help_assigned_lawyer_person_case_lawyer_help_pe~",
                        column: x => x.case_lawyer_help_person_id,
                        principalTable: "case_lawyer_help_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_case_session_act_assigned_~",
                table: "case_lawyer_help_assigned_lawyer",
                column: "case_session_act_assigned_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_person_case_lawyer_help_as~",
                table: "case_lawyer_help_assigned_lawyer_person",
                column: "case_lawyer_help_assigned_lawyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_person_case_lawyer_help_pe~",
                table: "case_lawyer_help_assigned_lawyer_person",
                column: "case_lawyer_help_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_assigned_lawyer_case_session_act_case_sess~",
                table: "case_lawyer_help_assigned_lawyer",
                column: "case_session_act_assigned_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_assigned_lawyer_case_session_act_case_sess~",
                table: "case_lawyer_help_assigned_lawyer");

            migrationBuilder.DropTable(
                name: "case_lawyer_help_assigned_lawyer_person");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_assigned_lawyer_case_session_act_assigned_~",
                table: "case_lawyer_help_assigned_lawyer");

            migrationBuilder.DropColumn(
                name: "case_session_act_assigned_id",
                table: "case_lawyer_help_assigned_lawyer");
        }
    }
}
