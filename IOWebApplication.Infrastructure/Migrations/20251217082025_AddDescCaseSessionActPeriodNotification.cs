using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescCaseSessionActPeriodNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "case_session_act_period_notification",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "case_session_act_period_notification");
        }
    }
}
