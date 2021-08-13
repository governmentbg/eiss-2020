using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class TableDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_table_description",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    table_name = table.Column<string>(nullable: true),
                    ordinal_position = table.Column<int>(nullable: false),
                    column_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    data_type = table.Column<string>(nullable: true),
                    data_type_normalized = table.Column<string>(nullable: true),
                    is_nullable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_table_description", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_table_description");
        }
    }
}
