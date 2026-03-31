using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApiKeyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_apikey",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    app_key = table.Column<string>(type: "text", nullable: true),
                    app_secret = table.Column<string>(type: "text", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remark = table.Column<string>(type: "text", nullable: true),
                    token_key = table.Column<string>(type: "text", nullable: true),
                    token_key_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    token_key_expire_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_apikey", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_apikey");
        }
    }
}
