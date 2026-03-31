using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EisppEktteCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eispp_ektte_code",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: true, comment: "ЕИСПП Код "),
                    district = table.Column<string>(type: "text", nullable: true, comment: "Област"),
                    municipality = table.Column<string>(type: "text", nullable: true, comment: "Община"),
                    type_nm = table.Column<string>(type: "text", nullable: true, comment: "тип н.м."),
                    name = table.Column<string>(type: "text", nullable: true, comment: "Населено място"),
                    rajon = table.Column<string>(type: "text", nullable: true, comment: "Район"),
                    system_code = table.Column<string>(type: "text", nullable: true, comment: "Системен идентификатор"),
                    system_name = table.Column<string>(type: "text", nullable: true, comment: "Системно име"),
                    ektte_code = table.Column<string>(type: "text", nullable: true, comment: "ЕКАТЕ"),
                    active = table.Column<string>(type: "text", nullable: true, comment: "Статус"),
                    date_from = table.Column<string>(type: "text", nullable: true, comment: "От дата"),
                    date_to = table.Column<string>(type: "text", nullable: true, comment: "До дата")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eispp_ektte_code", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eispp_ektte_code_code",
                table: "eispp_ektte_code",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eispp_ektte_code");
        }
    }
}
