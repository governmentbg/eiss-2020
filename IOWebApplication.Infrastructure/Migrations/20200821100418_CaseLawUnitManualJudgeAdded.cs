using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseLawUnitManualJudgeAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_lawunit_manual_judge",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    case_lawunit_id = table.Column<int>(nullable: false),
                    date_from = table.Column<DateTime>(nullable: false),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_lawunit_manual_judge", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_lawunit_manual_judge_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawunit_manual_judge_case_lawunit_case_lawunit_id",
                        column: x => x.case_lawunit_id,
                        principalTable: "case_lawunit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawunit_manual_judge_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawunit_manual_judge_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "case_lawunit_task_change",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    case_session_id = table.Column<int>(nullable: false),
                    work_task_id = table.Column<long>(nullable: false),
                    new_task_user_id = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_lawunit_task_change", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_lawunit_task_change_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawunit_task_change_case_session_act_case_session_id",
                        column: x => x.case_session_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawunit_task_change_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_lawunit_task_change_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_lawunit_task_change_common_worktask_work_task_id",
                        column: x => x.work_task_id,
                        principalTable: "common_worktask",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_manual_judge_case_id",
                table: "case_lawunit_manual_judge",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_manual_judge_case_lawunit_id",
                table: "case_lawunit_manual_judge",
                column: "case_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_manual_judge_court_id",
                table: "case_lawunit_manual_judge",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_manual_judge_user_id",
                table: "case_lawunit_manual_judge",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_task_change_case_id",
                table: "case_lawunit_task_change",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_task_change_case_session_id",
                table: "case_lawunit_task_change",
                column: "case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_task_change_court_id",
                table: "case_lawunit_task_change",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_task_change_user_id",
                table: "case_lawunit_task_change",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_task_change_work_task_id",
                table: "case_lawunit_task_change",
                column: "work_task_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_lawunit_manual_judge");

            migrationBuilder.DropTable(
                name: "case_lawunit_task_change");
        }
    }
}
