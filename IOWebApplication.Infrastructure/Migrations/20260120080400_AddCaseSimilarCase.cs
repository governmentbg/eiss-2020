using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseSimilarCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_read_similar_cases",
                table: "case_h",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_read_similar_cases",
                table: "case",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "case_similar_case",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на запис")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    case_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на дело"),
                    similar_case_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на дело")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_similar_case", x => x.id);
                    table.ForeignKey(
                        name: "FK_case_similar_case_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_similar_case_case_similar_case_id",
                        column: x => x.similar_case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Таблица със сходни дела за заповедно производство");

            migrationBuilder.CreateIndex(
                name: "IX_case_similar_case_case_id",
                table: "case_similar_case",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_case_similar_case_similar_case_id",
                table: "case_similar_case",
                column: "similar_case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_similar_case");

            migrationBuilder.DropColumn(
                name: "is_read_similar_cases",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "is_read_similar_cases",
                table: "case");
        }
    }
}
