using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class MigrationDataAddedMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MigrationDate",
                table: "common_migration_data",
                newName: "migration_date");

            migrationBuilder.AddColumn<string>(
                name: "message",
                table: "common_migration_data",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "message",
                table: "common_migration_data");

            migrationBuilder.RenameColumn(
                name: "migration_date",
                table: "common_migration_data",
                newName: "MigrationDate");
        }
    }
}
