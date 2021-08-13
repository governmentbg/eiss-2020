using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class VKS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vks_selection",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_department_id = table.Column<int>(nullable: false),
                    selection_year = table.Column<int>(nullable: false),
                    period_no = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vks_selection", x => x.id);
                    table.ForeignKey(
                        name: "FK_vks_selection_common_court_department_court_department_id",
                        column: x => x.court_department_id,
                        principalTable: "common_court_department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vks_selection_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vks_selection_lawunit",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    vks_selection_id = table.Column<int>(nullable: false),
                    lawunit_id = table.Column<int>(nullable: true),
                    lawunit_name = table.Column<string>(nullable: true),
                    is_unknown_judge = table.Column<bool>(nullable: false),
                    is_chairman_judge = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vks_selection_lawunit", x => x.id);
                    table.ForeignKey(
                        name: "FK_vks_selection_lawunit_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vks_selection_lawunit_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vks_selection_lawunit_vks_selection_vks_selection_id",
                        column: x => x.vks_selection_id,
                        principalTable: "vks_selection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vks_selection_month",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    vks_selection_id = table.Column<int>(nullable: false),
                    selection_month = table.Column<int>(nullable: false),
                    selection_day = table.Column<int>(nullable: false),
                    session_date = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vks_selection_month", x => x.id);
                    table.ForeignKey(
                        name: "FK_vks_selection_month_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vks_selection_month_vks_selection_vks_selection_id",
                        column: x => x.vks_selection_id,
                        principalTable: "vks_selection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vks_selection_month_lawunit",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    vks_selection_month_id = table.Column<int>(nullable: false),
                    vks_selection_lawunit_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vks_selection_month_lawunit", x => x.id);
                    table.ForeignKey(
                        name: "FK_vks_selection_month_lawunit_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vks_selection_month_lawunit_vks_selection_lawunit_vks_selec~",
                        column: x => x.vks_selection_lawunit_id,
                        principalTable: "vks_selection_lawunit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vks_selection_month_lawunit_vks_selection_month_vks_selecti~",
                        column: x => x.vks_selection_month_id,
                        principalTable: "vks_selection_month",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_case_session_act_id",
                table: "case_lawyer_help",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_court_department_id",
                table: "vks_selection",
                column: "court_department_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_user_id",
                table: "vks_selection",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_lawunit_lawunit_id",
                table: "vks_selection_lawunit",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_lawunit_user_id",
                table: "vks_selection_lawunit",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_lawunit_vks_selection_id",
                table: "vks_selection_lawunit",
                column: "vks_selection_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_month_user_id",
                table: "vks_selection_month",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_month_vks_selection_id",
                table: "vks_selection_month",
                column: "vks_selection_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_month_lawunit_user_id",
                table: "vks_selection_month_lawunit",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_month_lawunit_vks_selection_lawunit_id",
                table: "vks_selection_month_lawunit",
                column: "vks_selection_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_vks_selection_month_lawunit_vks_selection_month_id",
                table: "vks_selection_month_lawunit",
                column: "vks_selection_month_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_case_session_act_case_session_act_id",
                table: "case_lawyer_help",
                column: "case_session_act_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_case_session_act_case_session_act_id",
                table: "case_lawyer_help");

            migrationBuilder.DropTable(
                name: "vks_selection_month_lawunit");

            migrationBuilder.DropTable(
                name: "vks_selection_lawunit");

            migrationBuilder.DropTable(
                name: "vks_selection_month");

            migrationBuilder.DropTable(
                name: "vks_selection");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_case_session_act_id",
                table: "case_lawyer_help");
        }
    }
}
