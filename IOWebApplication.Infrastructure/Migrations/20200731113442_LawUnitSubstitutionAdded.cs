using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class LawUnitSubstitutionAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "law_unit_substitution_id",
                table: "case_lawunit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "common_court_lawunit_substitution",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_id = table.Column<int>(nullable: false),
                    lawunit_id = table.Column<int>(nullable: false),
                    substitute_lawunit_id = table.Column<int>(nullable: false),
                    date_from = table.Column<DateTime>(nullable: false),
                    date_to = table.Column<DateTime>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_court_lawunit_substitution", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_substitution_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_substitution_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_substitution_common_law_unit_substitut~",
                        column: x => x.substitute_lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_lawunit_substitution_identity_users_user_expir~",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_lawunit_law_unit_substitution_id",
                table: "case_lawunit",
                column: "law_unit_substitution_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_substitution_court_id",
                table: "common_court_lawunit_substitution",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_substitution_lawunit_id",
                table: "common_court_lawunit_substitution",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_substitution_substitute_lawunit_id",
                table: "common_court_lawunit_substitution",
                column: "substitute_lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_lawunit_substitution_user_expired_id",
                table: "common_court_lawunit_substitution",
                column: "user_expired_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawunit_common_court_lawunit_substitution_law_unit_sub~",
                table: "case_lawunit",
                column: "law_unit_substitution_id",
                principalTable: "common_court_lawunit_substitution",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawunit_common_court_lawunit_substitution_law_unit_sub~",
                table: "case_lawunit");

            migrationBuilder.DropTable(
                name: "common_court_lawunit_substitution");

            migrationBuilder.DropIndex(
                name: "IX_case_lawunit_law_unit_substitution_id",
                table: "case_lawunit");

            migrationBuilder.DropColumn(
                name: "law_unit_substitution_id",
                table: "case_lawunit");
        }
    }
}
