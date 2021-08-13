using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CasePersonCrimePunishment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_person_crime_punishment",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    case_case_person_id = table.Column<int>(nullable: false),
                    punishment_years = table.Column<int>(nullable: false),
                    punishment_months = table.Column<int>(nullable: false),
                    punishment_weeks = table.Column<int>(nullable: false),
                    punishment_days = table.Column<int>(nullable: false),
                    fine_amount = table.Column<double>(nullable: false),
                    punishment_kind = table.Column<int>(nullable: false),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_person_crime_punishment", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_person_crime_punishment_case_person_crimes_case_case_p~",
                        column: x => x.case_case_person_id,
                        principalTable: "case_person_crimes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_person_crime_punishment_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_case_person_crime_punishment_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_person_crime_punishment_case_case_person_id",
                table: "case_person_crime_punishment",
                column: "case_case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_crime_punishment_user_expired_id",
                table: "case_person_crime_punishment",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_crime_punishment_user_id",
                table: "case_person_crime_punishment",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_person_crime_punishment");
        }
    }
}
