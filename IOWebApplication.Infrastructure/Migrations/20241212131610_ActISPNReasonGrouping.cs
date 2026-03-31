using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActISPNReasonGrouping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_act_ispn_reason_grouping",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    act_ispn_reason_id = table.Column<int>(type: "integer", nullable: false),
                    act_ispn_reason_group = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_act_ispn_reason_grouping", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_act_ispn_reason_grouping_nom_act_ispn_reason_act_ispn_r~",
                        column: x => x.act_ispn_reason_id,
                        principalTable: "nom_act_ispn_reason",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_act_ispn_reason_grouping_act_ispn_reason_id",
                table: "nom_act_ispn_reason_grouping",
                column: "act_ispn_reason_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_act_ispn_reason_grouping");
        }
    }
}
