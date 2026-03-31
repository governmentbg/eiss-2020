using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditMediationPointMediatorAppraisalWithType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "multiplication_value",
                table: "nom_mediation_point_mediator_appraisal",
                type: "integer",
                nullable: true,
                comment: "Ако типа е 2, това е на каква оценка отговаря");

            migrationBuilder.AddColumn<int>(
                name: "type_point",
                table: "nom_mediation_point_mediator_appraisal",
                type: "integer",
                nullable: true,
                comment: "Тип оценка 1 - подробна / 2 - обобщена");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "multiplication_value",
                table: "nom_mediation_point_mediator_appraisal");

            migrationBuilder.DropColumn(
                name: "type_point",
                table: "nom_mediation_point_mediator_appraisal");
        }
    }
}
