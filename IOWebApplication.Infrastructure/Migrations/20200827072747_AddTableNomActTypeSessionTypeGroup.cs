using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddTableNomActTypeSessionTypeGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_act_type_session_type_group_nom_act_type_act_type_id",
                table: "act_type_session_type_group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_act_type_session_type_group",
                table: "act_type_session_type_group");

            migrationBuilder.RenameTable(
                name: "act_type_session_type_group",
                newName: "nom_act_type_session_type_group");

            migrationBuilder.RenameIndex(
                name: "IX_act_type_session_type_group_act_type_id",
                table: "nom_act_type_session_type_group",
                newName: "IX_nom_act_type_session_type_group_act_type_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nom_act_type_session_type_group",
                table: "nom_act_type_session_type_group",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_act_type_session_type_group_nom_act_type_act_type_id",
                table: "nom_act_type_session_type_group",
                column: "act_type_id",
                principalTable: "nom_act_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_act_type_session_type_group_nom_act_type_act_type_id",
                table: "nom_act_type_session_type_group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_nom_act_type_session_type_group",
                table: "nom_act_type_session_type_group");

            migrationBuilder.RenameTable(
                name: "nom_act_type_session_type_group",
                newName: "act_type_session_type_group");

            migrationBuilder.RenameIndex(
                name: "IX_nom_act_type_session_type_group_act_type_id",
                table: "act_type_session_type_group",
                newName: "IX_act_type_session_type_group_act_type_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_act_type_session_type_group",
                table: "act_type_session_type_group",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_act_type_session_type_group_nom_act_type_act_type_id",
                table: "act_type_session_type_group",
                column: "act_type_id",
                principalTable: "nom_act_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
