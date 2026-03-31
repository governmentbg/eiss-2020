using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MediationCaseMediatorAppraisalAddType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "type_appraisal",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: true,
                comment: "Тип оценка 1 - подробна / 2 - обобщена");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type_appraisal",
                table: "mediation_case_mediator_appraisal");
        }
    }
}
