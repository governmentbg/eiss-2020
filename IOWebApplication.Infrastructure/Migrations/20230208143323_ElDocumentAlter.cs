using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ElDocumentAlter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "electronic_document",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "money_fee_type_id",
                table: "electronic_document",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_money_fee_type_id",
                table: "electronic_document",
                column: "money_fee_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_electronic_document_nom_money_fee_type_money_fee_type_id",
                table: "electronic_document",
                column: "money_fee_type_id",
                principalTable: "nom_money_fee_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_electronic_document_nom_money_fee_type_money_fee_type_id",
                table: "electronic_document");

            migrationBuilder.DropIndex(
                name: "IX_electronic_document_money_fee_type_id",
                table: "electronic_document");

            migrationBuilder.DropColumn(
                name: "description",
                table: "electronic_document");

            migrationBuilder.DropColumn(
                name: "money_fee_type_id",
                table: "electronic_document");
        }
    }
}
