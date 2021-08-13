using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DeliveryTypeGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_delivery_type_group",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    notification_delivery_group_id = table.Column<int>(nullable: false),
                    notification_type_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_delivery_type_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_delivery_type_group_nom_notification_delivery_group_not~",
                        column: x => x.notification_delivery_group_id,
                        principalTable: "nom_notification_delivery_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_nom_delivery_type_group_nom_notification_type_notification_~",
                        column: x => x.notification_type_id,
                        principalTable: "nom_notification_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_delivery_type_group_notification_delivery_group_id",
                table: "nom_delivery_type_group",
                column: "notification_delivery_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_delivery_type_group_notification_type_id",
                table: "nom_delivery_type_group",
                column: "notification_type_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nom_delivery_type_group");
        }
    }
}
