using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditLabelCordinator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator_center",
                comment: "Центрове за медиация към координатор",
                oldComment: "Центрове за медиация към кординатор");

            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator",
                comment: "Координатори на центрове за медиация",
                oldComment: "Кординатори на центрове за медиация");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_coordinator_id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на координатор",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на кординатор");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator_center",
                comment: "Центрове за медиация към кординатор",
                oldComment: "Центрове за медиация към координатор");

            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator",
                comment: "Кординатори на центрове за медиация",
                oldComment: "Координатори на центрове за медиация");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_coordinator_id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на кординатор",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на координатор");
        }
    }
}
