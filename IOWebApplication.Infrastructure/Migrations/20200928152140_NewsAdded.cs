using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class NewsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_news",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    title = table.Column<string>(maxLength: 200, nullable: false),
                    content = table.Column<string>(nullable: false),
                    user_id = table.Column<string>(maxLength: 50, nullable: false),
                    publish_date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_news", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_news_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_news_user",
                columns: table => new
                {
                    news_id = table.Column<int>(nullable: false),
                    user_id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_news_user", x => new { x.news_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_common_news_user_common_news_news_id",
                        column: x => x.news_id,
                        principalTable: "common_news",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_news_user_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_news_user_id",
                table: "common_news",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_news_user_user_id",
                table: "common_news_user",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_news_user");

            migrationBuilder.DropTable(
                name: "common_news");
        }
    }
}
