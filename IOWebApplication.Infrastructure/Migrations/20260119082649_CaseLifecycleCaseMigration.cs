using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaseLifecycleCaseMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_migration_id",
                table: "case_lifecycle",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_lifecycle_case_migration_id",
                table: "case_lifecycle",
                column: "case_migration_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_lifecycle_case_migration_case_migration_id",
                table: "case_lifecycle",
                column: "case_migration_id",
                principalTable: "case_migration",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_lifecycle_case_migration_case_migration_id",
                table: "case_lifecycle");

            migrationBuilder.DropIndex(
                name: "IX_case_lifecycle_case_migration_id",
                table: "case_lifecycle");

            migrationBuilder.DropColumn(
                name: "case_migration_id",
                table: "case_lifecycle");
        }
    }
}
