using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FastProcessNomenclaturesCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "GroupCode",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "group_code");

            migrationBuilder.RenameColumn(
                name: "FastProcessClaimCircumstanceId",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "fastprocess_claim_circumstance_id");

            migrationBuilder.RenameIndex(
                name: "IX_nom_fastprocess_claim_circumstance_group_FastProcessClaimCi~",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "IX_nom_fastprocess_claim_circumstance_group_fastprocess_claim_~");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "group_code",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "GroupCode");

            migrationBuilder.RenameColumn(
                name: "fastprocess_claim_circumstance_id",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "FastProcessClaimCircumstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_nom_fastprocess_claim_circumstance_group_fastprocess_claim_~",
                table: "nom_fastprocess_claim_circumstance_group",
                newName: "IX_nom_fastprocess_claim_circumstance_group_FastProcessClaimCi~");
        }
    }
}
