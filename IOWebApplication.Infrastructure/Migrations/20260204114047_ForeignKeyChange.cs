using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_complain_r~",
                table: "case_session_act_selection_protocol_acts");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_result_act~",
                table: "case_session_act_selection_protocol_acts",
                column: "act_result_id",
                principalTable: "nom_act_result",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_result_act~",
                table: "case_session_act_selection_protocol_acts");

            migrationBuilder.AddForeignKey(
                name: "FK_case_session_act_selection_protocol_acts_nom_act_complain_r~",
                table: "case_session_act_selection_protocol_acts",
                column: "act_result_id",
                principalTable: "nom_act_complain_result",
                principalColumn: "id");
        }
    }
}
