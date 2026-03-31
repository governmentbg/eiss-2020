using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class AddFilterTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nom_filter_template_type",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    order_number = table.Column<int>(nullable: false),
                    code = table.Column<string>(nullable: true),
                    label = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    date_start = table.Column<DateTime>(nullable: false),
                    date_end = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_filter_template_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "common_filter_templates",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    filter_template_type_id = table.Column<int>(nullable: false),
                    label = table.Column<string>(nullable: false),
                    court_id = table.Column<int>(nullable: true),
                    for_user_id = table.Column<string>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_filter_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_filter_templates_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_common_filter_templates_nom_filter_template_type_filter_tem~",
                        column: x => x.filter_template_type_id,
                        principalTable: "nom_filter_template_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_filter_templates_identity_users_for_user_id",
                        column: x => x.for_user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_common_filter_templates_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_filter_templates_court_id",
                table: "common_filter_templates",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_filter_templates_filter_template_type_id",
                table: "common_filter_templates",
                column: "filter_template_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_filter_templates_for_user_id",
                table: "common_filter_templates",
                column: "for_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_filter_templates_user_id",
                table: "common_filter_templates",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_filter_templates");

            migrationBuilder.DropTable(
                name: "nom_filter_template_type");
        }
    }
}
