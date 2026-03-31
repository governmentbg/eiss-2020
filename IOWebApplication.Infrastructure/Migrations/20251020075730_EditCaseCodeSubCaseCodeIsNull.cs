using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditCaseCodeSubCaseCodeIsNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_code_sub_nom_case_code_case_code_id",
                table: "nom_case_code_sub");

            migrationBuilder.AlterColumn<int>(
                name: "case_code_id",
                table: "nom_case_code_sub",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на шифър",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на шифър");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_code_sub_nom_case_code_case_code_id",
                table: "nom_case_code_sub",
                column: "case_code_id",
                principalTable: "nom_case_code",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nom_case_code_sub_nom_case_code_case_code_id",
                table: "nom_case_code_sub");

            migrationBuilder.AlterColumn<int>(
                name: "case_code_id",
                table: "nom_case_code_sub",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на шифър",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на шифър");

            migrationBuilder.AddForeignKey(
                name: "FK_nom_case_code_sub_nom_case_code_case_code_id",
                table: "nom_case_code_sub",
                column: "case_code_id",
                principalTable: "nom_case_code",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
