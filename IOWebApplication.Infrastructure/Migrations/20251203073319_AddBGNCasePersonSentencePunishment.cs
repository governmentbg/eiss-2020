using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBGNCasePersonSentencePunishment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "sentence_money_bgn",
                table: "case_person_sentence_punishment_crime",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "sentence_money_bgn",
                table: "case_person_sentence_punishment",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sentence_money_bgn",
                table: "case_person_sentence_punishment_crime");

            migrationBuilder.DropColumn(
                name: "sentence_money_bgn",
                table: "case_person_sentence_punishment");
        }
    }
}
