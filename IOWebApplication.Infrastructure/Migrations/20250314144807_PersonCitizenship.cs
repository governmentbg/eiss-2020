using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersonCitizenship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "money_obligation_receive",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "money_obligation",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "electronic_document_person",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "document_person",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "common_person",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "common_law_unit_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "common_law_unit",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "common_institution",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "case_person_prev_name",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "case_person_h",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship_id",
                table: "case_person",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "electronic_document_person_address",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    electronic_document_person_id = table.Column<long>(type: "bigint", nullable: false),
                    address_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_electronic_document_person_address", x => x.id);
                    table.ForeignKey(
                        name: "FK_electronic_document_person_address_common_address_address_id",
                        column: x => x.address_id,
                        principalTable: "common_address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_electronic_document_person_address_electronic_document_pers~",
                        column: x => x.electronic_document_person_id,
                        principalTable: "electronic_document_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_money_obligation_receive_citizenship_id",
                table: "money_obligation_receive",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_money_obligation_citizenship_id",
                table: "money_obligation",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_person_citizenship_id",
                table: "electronic_document_person",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_person_citizenship_id",
                table: "document_person",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_person_citizenship_id",
                table: "common_person",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_law_unit_h_citizenship_id",
                table: "common_law_unit_h",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_law_unit_citizenship_id",
                table: "common_law_unit",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_institution_citizenship_id",
                table: "common_institution",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_prev_name_citizenship_id",
                table: "case_person_prev_name",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_h_citizenship_id",
                table: "case_person_h",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_person_citizenship_id",
                table: "case_person",
                column: "citizenship_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_person_address_address_id",
                table: "electronic_document_person_address",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_person_address_electronic_document_pers~",
                table: "electronic_document_person_address",
                column: "electronic_document_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_ek_countries_citizenship_id",
                table: "case_person",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_h_ek_countries_citizenship_id",
                table: "case_person_h",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_prev_name_ek_countries_citizenship_id",
                table: "case_person_prev_name",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_institution_ek_countries_citizenship_id",
                table: "common_institution",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_law_unit_ek_countries_citizenship_id",
                table: "common_law_unit",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_law_unit_h_ek_countries_citizenship_id",
                table: "common_law_unit_h",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_person_ek_countries_citizenship_id",
                table: "common_person",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_person_ek_countries_citizenship_id",
                table: "document_person",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_electronic_document_person_ek_countries_citizenship_id",
                table: "electronic_document_person",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_obligation_ek_countries_citizenship_id",
                table: "money_obligation",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_money_obligation_receive_ek_countries_citizenship_id",
                table: "money_obligation_receive",
                column: "citizenship_id",
                principalTable: "ek_countries",
                principalColumn: "country_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_ek_countries_citizenship_id",
                table: "case_person");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_h_ek_countries_citizenship_id",
                table: "case_person_h");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_prev_name_ek_countries_citizenship_id",
                table: "case_person_prev_name");

            migrationBuilder.DropForeignKey(
                name: "FK_common_institution_ek_countries_citizenship_id",
                table: "common_institution");

            migrationBuilder.DropForeignKey(
                name: "FK_common_law_unit_ek_countries_citizenship_id",
                table: "common_law_unit");

            migrationBuilder.DropForeignKey(
                name: "FK_common_law_unit_h_ek_countries_citizenship_id",
                table: "common_law_unit_h");

            migrationBuilder.DropForeignKey(
                name: "FK_common_person_ek_countries_citizenship_id",
                table: "common_person");

            migrationBuilder.DropForeignKey(
                name: "FK_document_person_ek_countries_citizenship_id",
                table: "document_person");

            migrationBuilder.DropForeignKey(
                name: "FK_electronic_document_person_ek_countries_citizenship_id",
                table: "electronic_document_person");

            migrationBuilder.DropForeignKey(
                name: "FK_money_obligation_ek_countries_citizenship_id",
                table: "money_obligation");

            migrationBuilder.DropForeignKey(
                name: "FK_money_obligation_receive_ek_countries_citizenship_id",
                table: "money_obligation_receive");

            migrationBuilder.DropTable(
                name: "electronic_document_person_address");

            migrationBuilder.DropIndex(
                name: "IX_money_obligation_receive_citizenship_id",
                table: "money_obligation_receive");

            migrationBuilder.DropIndex(
                name: "IX_money_obligation_citizenship_id",
                table: "money_obligation");

            migrationBuilder.DropIndex(
                name: "IX_electronic_document_person_citizenship_id",
                table: "electronic_document_person");

            migrationBuilder.DropIndex(
                name: "IX_document_person_citizenship_id",
                table: "document_person");

            migrationBuilder.DropIndex(
                name: "IX_common_person_citizenship_id",
                table: "common_person");

            migrationBuilder.DropIndex(
                name: "IX_common_law_unit_h_citizenship_id",
                table: "common_law_unit_h");

            migrationBuilder.DropIndex(
                name: "IX_common_law_unit_citizenship_id",
                table: "common_law_unit");

            migrationBuilder.DropIndex(
                name: "IX_common_institution_citizenship_id",
                table: "common_institution");

            migrationBuilder.DropIndex(
                name: "IX_case_person_prev_name_citizenship_id",
                table: "case_person_prev_name");

            migrationBuilder.DropIndex(
                name: "IX_case_person_h_citizenship_id",
                table: "case_person_h");

            migrationBuilder.DropIndex(
                name: "IX_case_person_citizenship_id",
                table: "case_person");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "money_obligation_receive");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "money_obligation");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "electronic_document_person");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "document_person");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "common_person");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "common_law_unit_h");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "common_law_unit");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "common_institution");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "case_person_prev_name");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "case_person_h");

            migrationBuilder.DropColumn(
                name: "citizenship_id",
                table: "case_person");
        }
    }
}
