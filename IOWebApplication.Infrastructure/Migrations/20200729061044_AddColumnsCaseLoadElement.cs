using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCaseLoadElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_act_type_act_type_id",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_result_session_~",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_type_session_ty~",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.AlterColumn<int>(
                name: "session_type_id",
                table: "nom_case_load_element_type_rule",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "session_result_id",
                table: "nom_case_load_element_type_rule",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "act_type_id",
                table: "nom_case_load_element_type_rule",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "is_create_motive",
                table: "nom_case_load_element_type_rule",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "replace_case_load_element_type_id",
                table: "nom_case_load_element_type",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "process_priority_id",
                table: "nom_case_load_element_group",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_type_replace_case_load_element_type_id",
                table: "nom_case_load_element_type",
                column: "replace_case_load_element_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_load_element_group_process_priority_id",
                table: "nom_case_load_element_group",
                column: "process_priority_id");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_group_nom_process_priority_process_pr~",
                table: "nom_case_load_element_group",
                column: "process_priority_id",
                principalTable: "nom_process_priority",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_nom_case_load_element_type_repla~",
                table: "nom_case_load_element_type",
                column: "replace_case_load_element_type_id",
                principalTable: "nom_case_load_element_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_act_type_act_type_id",
                table: "nom_case_load_element_type_rule",
                column: "act_type_id",
                principalTable: "nom_act_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_result_session_~",
                table: "nom_case_load_element_type_rule",
                column: "session_result_id",
                principalTable: "nom_session_result",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_type_session_ty~",
                table: "nom_case_load_element_type_rule",
                column: "session_type_id",
                principalTable: "nom_session_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_group_nom_process_priority_process_pr~",
                table: "nom_case_load_element_group");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_nom_case_load_element_type_repla~",
                table: "nom_case_load_element_type");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_act_type_act_type_id",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_result_session_~",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_type_session_ty~",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropIndex(
                name: "IX_nom_case_load_element_type_replace_case_load_element_type_id",
                table: "nom_case_load_element_type");

            migrationBuilder.DropIndex(
                name: "IX_nom_case_load_element_group_process_priority_id",
                table: "nom_case_load_element_group");

            migrationBuilder.DropColumn(
                name: "is_create_motive",
                table: "nom_case_load_element_type_rule");

            migrationBuilder.DropColumn(
                name: "replace_case_load_element_type_id",
                table: "nom_case_load_element_type");

            migrationBuilder.DropColumn(
                name: "process_priority_id",
                table: "nom_case_load_element_group");

            migrationBuilder.AlterColumn<int>(
                name: "session_type_id",
                table: "nom_case_load_element_type_rule",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "session_result_id",
                table: "nom_case_load_element_type_rule",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "act_type_id",
                table: "nom_case_load_element_type_rule",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_act_type_act_type_id",
                table: "nom_case_load_element_type_rule",
                column: "act_type_id",
                principalTable: "nom_act_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_result_session_~",
                table: "nom_case_load_element_type_rule",
                column: "session_result_id",
                principalTable: "nom_session_result",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_load_element_type_rule_nom_session_type_session_ty~",
                table: "nom_case_load_element_type_rule",
                column: "session_type_id",
                principalTable: "nom_session_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
