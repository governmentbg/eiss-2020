using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class PersonRoleGrouping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "claim_number",
                table: "case_money_claim",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateTable(
                name: "nom_person_role_grouping",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    person_role_id = table.Column<int>(nullable: false),
                    person_role_group = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_person_role_grouping", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_person_role_grouping_nom_person_role_person_role_id",
                        column: x => x.person_role_id,
                        principalTable: "nom_person_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_person_role_grouping_person_role_id",
                table: "nom_person_role_grouping",
                column: "person_role_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_person_role_grouping");

            migrationBuilder.AlterColumn<string>(
                name: "claim_number",
                table: "case_money_claim",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
