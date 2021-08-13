using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddTableLawyerHelpBaseCaseGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_lawyer_help_base_case_group",
                columns: table => new
                {
                    lawyer_help_base_id = table.Column<int>(nullable: false),
                    case_group_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_lawyer_help_base_case_group", x => new { x.lawyer_help_base_id, x.case_group_id });
                    table.ForeignKey(
                        name: "FK_nom_lawyer_help_base_case_group_nom_case_group_case_group_id",
                        column: x => x.case_group_id,
                        principalTable: "nom_case_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_lawyer_help_base_case_group_nom_lawyer_help_base_lawyer~",
                        column: x => x.lawyer_help_base_id,
                        principalTable: "nom_lawyer_help_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_lawyer_help_base_case_group_case_group_id",
                table: "nom_lawyer_help_base_case_group",
                column: "case_group_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_lawyer_help_base_case_group");
        }
    }
}
