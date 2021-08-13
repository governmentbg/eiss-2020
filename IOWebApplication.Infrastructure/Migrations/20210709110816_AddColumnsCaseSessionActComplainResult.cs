using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnsCaseSessionActComplainResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_complain_result_case_session_act_case_sess~",
                table: "case_session_act_complain_result");

            migrationBuilder.AlterColumn<int>(
                name: "case_session_act_id",
                table: "case_session_act_complain_result",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "case_reg_number_other_system",
                table: "case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "case_session_act_other_system",
                table: "case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "case_short_number_other_system",
                table: "case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_year_other_system",
                table: "case_session_act_complain_result",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_complain_result_case_session_act_case_sess~",
                table: "case_session_act_complain_result",
                column: "case_session_act_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_complain_result_case_session_act_case_sess~",
                table: "case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "case_reg_number_other_system",
                table: "case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "case_session_act_other_system",
                table: "case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "case_short_number_other_system",
                table: "case_session_act_complain_result");

            migrationBuilder.DropColumn(
                name: "case_year_other_system",
                table: "case_session_act_complain_result");

            migrationBuilder.AlterColumn<int>(
                name: "case_session_act_id",
                table: "case_session_act_complain_result",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_complain_result_case_session_act_case_sess~",
                table: "case_session_act_complain_result",
                column: "case_session_act_id",
                principalTable: "case_session_act",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
