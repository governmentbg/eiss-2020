using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddTableActTypeSessionTypeGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "act_type_session_type_group",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    act_type_id = table.Column<int>(nullable: false),
                    session_type_group = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_act_type_session_type_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_act_type_session_type_group_nom_act_type_act_type_id",
                        column: x => x.act_type_id,
                        principalTable: "nom_act_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_act_type_session_type_group_act_type_id",
                table: "act_type_session_type_group",
                column: "act_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "act_type_session_type_group");
        }
    }
}
