using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class CasePersonMeasure_MQEpep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mq_epep_id",
                table: "case_person_measures",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "mq_epep_is_send",
                table: "case_person_measures",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mq_epep_id",
                table: "case_person_measures");

            migrationBuilder.DropColumn(
                name: "mq_epep_is_send",
                table: "case_person_measures");
        }
    }
}
