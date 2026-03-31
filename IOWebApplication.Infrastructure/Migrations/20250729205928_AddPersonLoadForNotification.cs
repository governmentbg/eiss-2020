using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonLoadForNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_fast_process",
                table: "nom_work_notification_type",
                type: "boolean",
                nullable: true,
                comment: "Флаг дали нотификацията е за бързо производство",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "calendar_color",
                table: "nom_work_notification_type",
                type: "text",
                nullable: true,
                comment: "Цвят за календар",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "type_person_load",
                table: "nom_work_notification_type",
                type: "integer",
                nullable: true,
                comment: "Служители за нотификация - 1 - Съдебен състав (без ръчни роли) / 2 - Служители (само ръчни роли) / 3 - само съдия докладчик / всички");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type_person_load",
                table: "nom_work_notification_type");

            migrationBuilder.AlterColumn<bool>(
                name: "is_fast_process",
                table: "nom_work_notification_type",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true,
                oldComment: "Флаг дали нотификацията е за бързо производство");

            migrationBuilder.AlterColumn<string>(
                name: "calendar_color",
                table: "nom_work_notification_type",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Цвят за календар");
        }
    }
}
