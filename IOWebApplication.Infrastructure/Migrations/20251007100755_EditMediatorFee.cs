using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditMediatorFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mediation_type_id",
                table: "common_mediator_fee",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на вид среща за медиация");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediator_fee_mediation_type_id",
                table: "common_mediator_fee",
                column: "mediation_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_mediator_fee_nom_mediation_type_mediation_type_id",
                table: "common_mediator_fee",
                column: "mediation_type_id",
                principalTable: "nom_mediation_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_mediator_fee_nom_mediation_type_mediation_type_id",
                table: "common_mediator_fee");

            migrationBuilder.DropIndex(
                name: "IX_common_mediator_fee_mediation_type_id",
                table: "common_mediator_fee");

            migrationBuilder.DropColumn(
                name: "mediation_type_id",
                table: "common_mediator_fee");
        }
    }
}
