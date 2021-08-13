using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VKS_20210416 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_chairman_judge",
                table: "vks_selection_lawunit");

            migrationBuilder.DropColumn(
                name: "period_no",
                table: "vks_selection");

            migrationBuilder.RenameColumn(
                name: "selection_year",
                table: "vks_selection",
                newName: "vks_selection_header_id");

            migrationBuilder.AddColumn<int>(
                name: "judge_department_role_id",
                table: "vks_selection_lawunit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nom_vks_selection_state",
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
                    table.PrimaryKey("PK_nom_vks_selection_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vks_selection_header",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    kolegia_id = table.Column<int>(nullable: false),
                    selection_year = table.Column<int>(nullable: false),
                    period_no = table.Column<int>(nullable: false),
                    months = table.Column<string>(nullable: true),
                    vks_selection_state_id = table.Column<int>(nullable: false),
                    state_date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vks_selection_header", x => x.id);
                    table.ForeignKey(
                        name: "FK_vks_selection_header_common_court_department_kolegia_id",
                        column: x => x.kolegia_id,
                        principalTable: "common_court_department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vks_selection_header_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vks_selection_header_nom_vks_selection_state_vks_selection_~",
                        column: x => x.vks_selection_state_id,
                        principalTable: "nom_vks_selection_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_lawunit_judge_department_role_id",
                table: "vks_selection_lawunit",
                column: "judge_department_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_vks_selection_header_id",
                table: "vks_selection",
                column: "vks_selection_header_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_header_kolegia_id",
                table: "vks_selection_header",
                column: "kolegia_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_header_user_id",
                table: "vks_selection_header",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_header_vks_selection_state_id",
                table: "vks_selection_header",
                column: "vks_selection_state_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vks_selection_vks_selection_header_vks_selection_header_id",
                table: "vks_selection",
                column: "vks_selection_header_id",
                principalTable: "vks_selection_header",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vks_selection_lawunit_nom_judge_department_role_judge_depar~",
                table: "vks_selection_lawunit",
                column: "judge_department_role_id",
                principalTable: "nom_judge_department_role",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vks_selection_vks_selection_header_vks_selection_header_id",
                table: "vks_selection");

            migrationBuilder.DropForeignKey(
                name: "FK_vks_selection_lawunit_nom_judge_department_role_judge_depar~",
                table: "vks_selection_lawunit");

            migrationBuilder.DropTable(
                name: "vks_selection_header");

            migrationBuilder.DropTable(
                name: "nom_vks_selection_state");

            migrationBuilder.DropIndex(
                name: "IX_vks_selection_lawunit_judge_department_role_id",
                table: "vks_selection_lawunit");

            migrationBuilder.DropIndex(
                name: "IX_vks_selection_vks_selection_header_id",
                table: "vks_selection");

            migrationBuilder.DropColumn(
                name: "judge_department_role_id",
                table: "vks_selection_lawunit");

            migrationBuilder.RenameColumn(
                name: "vks_selection_header_id",
                table: "vks_selection",
                newName: "selection_year");

            migrationBuilder.AddColumn<bool>(
                name: "is_chairman_judge",
                table: "vks_selection_lawunit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "period_no",
                table: "vks_selection",
                nullable: false,
                defaultValue: 0);
        }
    }
}
