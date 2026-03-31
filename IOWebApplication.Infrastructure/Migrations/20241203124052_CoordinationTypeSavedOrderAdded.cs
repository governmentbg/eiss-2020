using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CoordinationTypeSavedOrderAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "coordination_type",
                table: "case_session_act_coordination",
                type: "integer",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.AddColumn<int>(
                name: "saved_order_by",
                table: "case_lawunit",
                type: "integer",
                nullable: false,
                defaultValueSql: "0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coordination_type",
                table: "case_session_act_coordination");

            migrationBuilder.DropColumn(
                name: "saved_order_by",
                table: "case_lawunit");
        }
    }
}
