using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddColumnCaseSessionActIdCaseLoadIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "epep_user",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "case_session_act_id",
                table: "case_load_index",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "case_session_act_id",
                table: "case_load_index");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "epep_user",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
