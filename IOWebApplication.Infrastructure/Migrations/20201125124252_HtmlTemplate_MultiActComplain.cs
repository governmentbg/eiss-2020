using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class HtmlTemplate_MultiActComplain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "have_act_complain_free",
                table: "common_html_template",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "have_multi_act_complain",
                table: "common_html_template",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "have_act_complain_free",
                table: "common_html_template");

            migrationBuilder.DropColumn(
                name: "have_multi_act_complain",
                table: "common_html_template");
        }
    }
}
