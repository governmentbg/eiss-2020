using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediationFeeAndSubCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "case_code_sub_id",
                table: "case_h",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на подшифри в дело");

            migrationBuilder.AddColumn<int>(
                name: "case_code_sub_id",
                table: "case",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на подшифри в дело");

            migrationBuilder.CreateTable(
                name: "common_mediator_fee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификаотр на записа")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hour_fee = table.Column<decimal>(type: "numeric", nullable: false, comment: "Възнаграждение на час"),
                    hour_fee_eur = table.Column<decimal>(type: "numeric", nullable: false, comment: "Възнаграждение на час в евро"),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата от"),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата до"),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_mediator_fee", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_mediator_fee_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                },
                comment: "Ставки за заплащане на медиатори");

            migrationBuilder.CreateTable(
                name: "nom_case_code_sub",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_code_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на шифър"),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_case_code_sub", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_case_code_sub_nom_case_code_case_code_id",
                        column: x => x.case_code_id,
                        principalTable: "nom_case_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Подшифри в дело");

            migrationBuilder.CreateIndex(
                name: "IX_case_case_code_sub_id",
                table: "case",
                column: "case_code_sub_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediator_fee_user_id",
                table: "common_mediator_fee",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_case_code_sub_case_code_id",
                table: "nom_case_code_sub",
                column: "case_code_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_nom_case_code_sub_case_code_sub_id",
                table: "case",
                column: "case_code_sub_id",
                principalTable: "nom_case_code_sub",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_nom_case_code_sub_case_code_sub_id",
                table: "case");

            migrationBuilder.DropTable(
                name: "common_mediator_fee");

            migrationBuilder.DropTable(
                name: "nom_case_code_sub");

            migrationBuilder.DropIndex(
                name: "IX_case_case_code_sub_id",
                table: "case");

            migrationBuilder.DropColumn(
                name: "case_code_sub_id",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "case_code_sub_id",
                table: "case");
        }
    }
}
