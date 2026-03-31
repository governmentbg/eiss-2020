using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditMediationPointMediatorAppraisal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "description_expired",
                table: "nom_mediation_point_mediator_appraisal",
                newName: "without_appraisal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "without_appraisal",
                table: "nom_mediation_point_mediator_appraisal",
                newName: "description_expired");
        }
    }
}
