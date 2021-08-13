using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CaseDeactivation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_deactivation",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    court_id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    deactivate_user_id = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    declared_date = table.Column<DateTime>(nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_deactivation", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_deactivation_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_deactivation_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_deactivation_identity_users_deactivate_user_id",
                        column: x => x.deactivate_user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_deactivation_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_deactivation_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "id_list",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    remark = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_id_list", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_deactivation_case_id",
                table: "case_deactivation",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_deactivation_court_id",
                table: "case_deactivation",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_deactivation_deactivate_user_id",
                table: "case_deactivation",
                column: "deactivate_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_deactivation_user_expired_id",
                table: "case_deactivation",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_deactivation_user_id",
                table: "case_deactivation",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_deactivation");

            migrationBuilder.DropTable(
                name: "id_list");
        }
    }
}
