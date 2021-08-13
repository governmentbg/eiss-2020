using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class DocumentNotificationMLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document_notification_mlink",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    court_id = table.Column<int>(nullable: true),
                    document_id = table.Column<long>(nullable: true),
                    document_notification_id = table.Column<int>(nullable: false),
                    document_person_id = table.Column<long>(nullable: true),
                    document_resolution_id = table.Column<long>(nullable: true),
                    document_person_summoned_id = table.Column<long>(nullable: true),
                    document_person_link_id = table.Column<int>(nullable: true),
                    person_name = table.Column<string>(nullable: true),
                    person_role = table.Column<string>(nullable: true),
                    is_checked = table.Column<bool>(nullable: false),
                    is_active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_notification_mlink", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_notification_mlink_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_mlink_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_mlink_document_notification_document_~",
                        column: x => x.document_notification_id,
                        principalTable: "document_notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_document_notification_mlink_document_person_document_person~",
                        column: x => x.document_person_id,
                        principalTable: "document_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_mlink_document_person_document_perso~1",
                        column: x => x.document_person_summoned_id,
                        principalTable: "document_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_notification_mlink_document_resolution_document_re~",
                        column: x => x.document_resolution_id,
                        principalTable: "document_resolution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_mlink_court_id",
                table: "document_notification_mlink",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_mlink_document_id",
                table: "document_notification_mlink",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_mlink_document_notification_id",
                table: "document_notification_mlink",
                column: "document_notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_mlink_document_person_id",
                table: "document_notification_mlink",
                column: "document_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_mlink_document_person_summoned_id",
                table: "document_notification_mlink",
                column: "document_person_summoned_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_notification_mlink_document_resolution_id",
                table: "document_notification_mlink",
                column: "document_resolution_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_notification_mlink");
        }
    }
}
