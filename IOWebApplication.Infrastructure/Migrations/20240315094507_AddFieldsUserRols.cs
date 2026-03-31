using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddFieldsUserRols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "identity_roles",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "label",
                table: "identity_roles",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "order_number",
                table: "identity_roles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "identity_roles");

            migrationBuilder.DropColumn(
                name: "label",
                table: "identity_roles");

            migrationBuilder.DropColumn(
                name: "order_number",
                table: "identity_roles");
        }
    }
}
