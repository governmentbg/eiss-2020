using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddFieldsCaseLawyerHelp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "specified_lawyer",
                table: "case_lawyer_help_person",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_session_to_go_id",
                table: "case_lawyer_help",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_lawyer_help_case_session_to_go_id",
                table: "case_lawyer_help",
                column: "case_session_to_go_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lawyer_help_case_session_case_session_to_go_id",
                table: "case_lawyer_help",
                column: "case_session_to_go_id",
                principalTable: "case_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lawyer_help_case_session_case_session_to_go_id",
                table: "case_lawyer_help");

            migrationBuilder.DropIndex(
                name: "IX_case_lawyer_help_case_session_to_go_id",
                table: "case_lawyer_help");

            migrationBuilder.DropColumn(
                name: "specified_lawyer",
                table: "case_lawyer_help_person");

            migrationBuilder.DropColumn(
                name: "case_session_to_go_id",
                table: "case_lawyer_help");
        }
    }
}
