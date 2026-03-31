using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class LogElectionAddCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "election_person_id",
                table: "election_log",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_election_person_election_person_dismissal_type_id",
                table: "election_person",
                column: "election_person_dismissal_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_election_log_election_person_id",
                table: "election_log",
                column: "election_person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_election_log_election_person_election_person_id",
                table: "election_log",
                column: "election_person_id",
                principalTable: "election_person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_election_person_nom_election_person_dismissal_type_election~",
                table: "election_person",
                column: "election_person_dismissal_type_id",
                principalTable: "nom_election_person_dismissal_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_election_log_election_person_election_person_id",
                table: "election_log");

            migrationBuilder.DropForeignKey(
                name: "FK_election_person_nom_election_person_dismissal_type_election~",
                table: "election_person");

            migrationBuilder.DropIndex(
                name: "IX_election_person_election_person_dismissal_type_id",
                table: "election_person");

            migrationBuilder.DropIndex(
                name: "IX_election_log_election_person_id",
                table: "election_log");

            migrationBuilder.DropColumn(
                name: "election_person_id",
                table: "election_log");
        }
    }
}
