using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAmmentCourtDuty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "common_court_duty",
                comment: "Дежурства към съд");

            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "common_court_duty",
                type: "text",
                nullable: false,
                comment: "Наименование",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_court_duty",
                type: "text",
                nullable: true,
                comment: "Описание",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_court_duty",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата до",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_court_duty",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Дата от",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "common_court_duty",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на съд",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "act_number",
                table: "common_court_duty",
                type: "text",
                nullable: true,
                comment: "Номер заповед",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "act_date",
                table: "common_court_duty",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата заповед",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_court_duty",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на записа",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "common_court_duty",
                oldComment: "Дежурства към съд");

            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "common_court_duty",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Наименование");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_court_duty",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Описание");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_court_duty",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата до");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_court_duty",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата от");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "common_court_duty",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на съд");

            migrationBuilder.AlterColumn<string>(
                name: "act_number",
                table: "common_court_duty",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Номер заповед");

            migrationBuilder.AlterColumn<DateTime>(
                name: "act_date",
                table: "common_court_duty",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата заповед");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_court_duty",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на записа")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
