using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseSessionActPeriodNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_session_act_period_notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на записа")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_session_act_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на акт"),
                    work_notification_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип нотификация"),
                    notification_on = table.Column<bool>(type: "boolean", nullable: true, comment: "Флаг дали да се създаде нотификация"),
                    notification_days = table.Column<int>(type: "integer", nullable: true, comment: "Нотификация след дни"),
                    notification_weeks = table.Column<int>(type: "integer", nullable: true, comment: "Нотификация след седмици"),
                    notification_months = table.Column<int>(type: "integer", nullable: true, comment: "Нотификация след месеци")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_session_act_period_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_session_act_period_notification_case_session_act_case_~",
                        column: x => x.case_session_act_id,
                        principalTable: "case_session_act",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_session_act_period_notification_nom_work_notification_~",
                        column: x => x.work_notification_type_id,
                        principalTable: "nom_work_notification_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Таблица със срокове за нотификации към акт");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_period_notification_case_session_act_id",
                table: "case_session_act_period_notification",
                column: "case_session_act_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_session_act_period_notification_work_notification_type~",
                table: "case_session_act_period_notification",
                column: "work_notification_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_session_act_period_notification");
        }
    }
}
