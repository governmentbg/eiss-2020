using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ElectronicDocumentAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "person_gid",
                table: "electronic_document_person",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "represents_gid",
                table: "electronic_document_person",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "paid_date",
                table: "electronic_document",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "payment_type_id",
                table: "electronic_document",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "request_type_code",
                table: "electronic_document",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "assignment_document_id",
                table: "document",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "court_id",
                table: "common_main_transaction",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_payment_type_id",
                table: "electronic_document",
                column: "payment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_assignment_document_id",
                table: "document",
                column: "assignment_document_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_document_assignment_document_id",
                table: "document",
                column: "assignment_document_id",
                principalTable: "document",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_electronic_document_nom_payment_type_payment_type_id",
                table: "electronic_document",
                column: "payment_type_id",
                principalTable: "nom_payment_type",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_document_assignment_document_id",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "FK_electronic_document_nom_payment_type_payment_type_id",
                table: "electronic_document");

            migrationBuilder.DropIndex(
                name: "IX_electronic_document_payment_type_id",
                table: "electronic_document");

            migrationBuilder.DropIndex(
                name: "IX_document_assignment_document_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "person_gid",
                table: "electronic_document_person");

            migrationBuilder.DropColumn(
                name: "represents_gid",
                table: "electronic_document_person");

            migrationBuilder.DropColumn(
                name: "payment_type_id",
                table: "electronic_document");

            migrationBuilder.DropColumn(
                name: "request_type_code",
                table: "electronic_document");

            migrationBuilder.DropColumn(
                name: "assignment_document_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "court_id",
                table: "common_main_transaction");

            migrationBuilder.AlterColumn<DateTime>(
                name: "paid_date",
                table: "electronic_document",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
