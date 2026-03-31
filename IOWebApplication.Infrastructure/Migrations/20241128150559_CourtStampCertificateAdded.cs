using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CourtStampCertificateAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_court_stampt_certificate",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: false),
                    date_uploaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    serial_number = table.Column<string>(type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    pass_hash = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<byte[]>(type: "bytea", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_court_stampt_certificate", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_court_stampt_certificate_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_court_stampt_certificate_identity_users_user_expired~",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_court_stampt_certificate_court_id",
                table: "common_court_stampt_certificate",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_court_stampt_certificate_user_expired_id",
                table: "common_court_stampt_certificate",
                column: "user_expired_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_court_stampt_certificate");
        }
    }
}
