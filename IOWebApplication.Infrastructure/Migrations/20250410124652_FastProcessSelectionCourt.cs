using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FastProcessSelectionCourt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_fast_process_selection_court",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: false),
                    year_sel = table.Column<int>(type: "integer", nullable: false),
                    month_sel = table.Column<int>(type: "integer", nullable: false),
                    day_sel = table.Column<int>(type: "integer", nullable: false),
                    selection_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_target = table.Column<int>(type: "integer", nullable: false),
                    added_after_zero = table.Column<int>(type: "integer", nullable: false),
                    selected_to_now = table.Column<int>(type: "integer", nullable: false),
                    selected_for_day = table.Column<int>(type: "integer", nullable: false),
                    left_for_selection = table.Column<int>(type: "integer", nullable: false),
                    simulation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_fast_process_selection_court", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_fast_process_selection_court_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_fast_process_selection_court_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_fast_process_selection_court_court_id",
                table: "common_fast_process_selection_court",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_fast_process_selection_court_user_id",
                table: "common_fast_process_selection_court",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_fast_process_selection_court");
        }
    }
}
