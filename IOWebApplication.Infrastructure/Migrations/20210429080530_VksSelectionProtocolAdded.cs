using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VksSelectionProtocolAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "court_department_type_id",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vks_selection_protocol",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    vks_selection_id = table.Column<int>(nullable: false),
                    date_generated = table.Column<DateTime>(nullable: false),
                    user_generated = table.Column<string>(nullable: true),
                    date_signed = table.Column<DateTime>(nullable: true),
                    user_signed = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vks_selection_protocol", x => x.id);
                    table.ForeignKey(
                        name: "FK_vks_selection_protocol_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vks_selection_protocol_vks_selection_vks_selection_id",
                        column: x => x.vks_selection_id,
                        principalTable: "vks_selection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_lawunit_court_department_type_id",
                table: "vks_selection_lawunit",
                column: "court_department_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_protocol_user_id",
                table: "vks_selection_protocol",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_protocol_vks_selection_id",
                table: "vks_selection_protocol",
                column: "vks_selection_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vks_selection_lawunit_nom_department_type_court_department_~",
                table: "vks_selection_lawunit",
                column: "court_department_type_id",
                principalTable: "nom_department_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vks_selection_lawunit_nom_department_type_court_department_~",
                table: "vks_selection_lawunit");

            migrationBuilder.DropTable(
                name: "vks_selection_protocol");

            migrationBuilder.DropIndex(
                name: "IX_vks_selection_lawunit_court_department_type_id",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "court_department_type_id",
                table: "vks_selection_lawunit");
        }
    }
}
