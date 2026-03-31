using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentMediation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "nom_mediation_type",
                comment: "Вид среща за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_state",
                comment: "Статуси на среща за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_result_group",
                comment: "Групи за резултат от срещи за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_result_base",
                comment: "Основания за резултати от срещи за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_result",
                comment: "Резултат от срещи за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_point_mediator_appraisal",
                comment: "Точки за оценяване на медиатори за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_person_session_state",
                comment: "Статус на лицето след срещата - присъства и т.н. за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_location",
                comment: "Място на което се провеждат срещите за медиация");

            migrationBuilder.AlterTable(
                name: "mediation_case_session_result",
                comment: "Резултати в среща за медиация");

            migrationBuilder.AlterTable(
                name: "mediation_case_session",
                comment: "Среща по дело - медиация");

            migrationBuilder.AlterTable(
                name: "mediation_case_person",
                comment: "Страни по делото за срещата");

            migrationBuilder.AlterTable(
                name: "mediation_case_mediator_appraisal_data",
                comment: "Таблица с подробни данни за оценка на медиатори към дело");

            migrationBuilder.AlterTable(
                name: "mediation_case_mediator_appraisal",
                comment: "Таблица с оценка на медиатори към дело");

            migrationBuilder.AlterTable(
                name: "mediation_case_mediator",
                comment: "Таблица с медиатори към дело");

            migrationBuilder.AlterTable(
                name: "common_mediation_mediator_center",
                comment: "Центрове към медиатор");

            migrationBuilder.AlterTable(
                name: "common_mediation_mediator",
                comment: "Медиатори");

            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator_center",
                comment: "Центрове за медиация към кординатор");

            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator",
                comment: "Кординатори на центрове за медиация");

            migrationBuilder.AlterTable(
                name: "common_mediation_center_court",
                comment: "Кои съдилища се обслужват от даден център");

            migrationBuilder.AlterTable(
                name: "common_mediation_center",
                comment: "Центрове за медиация");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_group_id",
                table: "nom_mediation_result_base",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на група",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_group_id",
                table: "nom_mediation_result",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на група",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "without_appraisal",
                table: "nom_mediation_point_mediator_appraisal",
                type: "boolean",
                nullable: false,
                comment: "Флаг дали е ред на който не се дава оценка",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_session_result",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на резултат от среща",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_base_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на основание за резултат от среща",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_session_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на среща",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "is_main",
                table: "mediation_case_session_result",
                type: "boolean",
                nullable: false,
                comment: "Флаг за основен резултат",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_session_result",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_session_result",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_session_result",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на съд",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на дело",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_type_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на вид среща: Информационна среща; Процедура по медиация.",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_state_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                comment: "Статус на срещата",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_location_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на място на което се провежда",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "mediation_location_description",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                comment: "Допълнително пояснение за място на което се провежда",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "mediation_case_session",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Край: дата и час",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "mediation_case_session",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Начало: дата и час",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_session",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на съд",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на дело",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_session",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_person",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_person_session_state_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на статус на лицето след срещата - присъства и т.н.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_session_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на среща",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_person",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_person",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_person",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на съд",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "case_person_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на страна от делото",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на дело",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "rating",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                comment: "Оценка",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_point_mediator_appraisal_id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на точка за оценяване",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_mediator_appraisal_id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на медиатор в дело",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_mediator_appraisal",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_mediator_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на медиатор в дело",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_mediator_appraisal",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_mediator_appraisal",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_mediator_appraisal",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_appraisal",
                table: "mediation_case_mediator_appraisal",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Дата на оценка",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на съд",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на дело",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_mediator",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_mediator_id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на медиатор",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_mediator",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_mediator",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "mediation_case_mediator",
                type: "timestamp with time zone",
                nullable: true,
                comment: "До",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "mediation_case_mediator",
                type: "timestamp with time zone",
                nullable: false,
                comment: "От",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_mediator",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на съд",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на дело",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_mediator_center",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_mediator_id",
                table: "common_mediation_mediator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на медиатор",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_centers_id",
                table: "common_mediation_mediator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на център",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_mediator_center",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_mediator_center",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_mediator_center",
                type: "timestamp with time zone",
                nullable: true,
                comment: "До",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_mediator_center",
                type: "timestamp with time zone",
                nullable: false,
                comment: "От",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_mediator_center",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_mediator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "practice_specific_area_law_year",
                table: "common_mediation_mediator",
                type: "integer",
                nullable: true,
                comment: "Практика в определена област на правото",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Имена на медиатора",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "main_profession",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Основна професия",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "experience_mediation",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Опит на медиатора в медиация по опредени видове спорове",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "education",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Образование",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: true,
                comment: "До дата",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: false,
                comment: "От дата",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_entry",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на вписване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "additional_qualification",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                comment: "Допълнителна квалификация",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_mediator",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_coordinator_center",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_coordinator_id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на кординатор",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_centers_id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на център",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_coordinator_center",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_coordinator_center",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_coordinator_center",
                type: "timestamp with time zone",
                nullable: true,
                comment: "До",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_coordinator_center",
                type: "timestamp with time zone",
                nullable: false,
                comment: "От",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_coordinator_center",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "position",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                comment: "Длъжност",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "lawunit_user_id",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребителя на лицето",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "lawunit_id",
                table: "common_mediation_coordinator",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на лице",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                comment: "Забележка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_coordinator",
                type: "timestamp with time zone",
                nullable: true,
                comment: "До",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_coordinator",
                type: "timestamp with time zone",
                nullable: false,
                comment: "От",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_coordinator",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_coordinator",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_center_id",
                table: "common_mediation_center_court",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на център за медиация",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "common_mediation_center_court",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на съд",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_center_court",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                comment: "Идентификатор на потребител, който е анулирал записа",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "parent_id",
                table: "common_mediation_center",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на родител",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "common_mediation_center",
                type: "text",
                nullable: false,
                comment: "Име на центъра",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                comment: "Причина за анулиране",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                comment: "Описание",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_center",
                type: "timestamp with time zone",
                nullable: true,
                comment: "До дата",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_center",
                type: "timestamp with time zone",
                nullable: false,
                comment: "От дата",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_center",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на анулиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "contact_details",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                comment: "Контактни данни",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "address_text",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                comment: "Адрес",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_center",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на запис",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "nom_mediation_type",
                oldComment: "Вид среща за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_state",
                oldComment: "Статуси на среща за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_result_group",
                oldComment: "Групи за резултат от срещи за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_result_base",
                oldComment: "Основания за резултати от срещи за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_result",
                oldComment: "Резултат от срещи за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_point_mediator_appraisal",
                oldComment: "Точки за оценяване на медиатори за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_person_session_state",
                oldComment: "Статус на лицето след срещата - присъства и т.н. за медиация");

            migrationBuilder.AlterTable(
                name: "nom_mediation_location",
                oldComment: "Място на което се провеждат срещите за медиация");

            migrationBuilder.AlterTable(
                name: "mediation_case_session_result",
                oldComment: "Резултати в среща за медиация");

            migrationBuilder.AlterTable(
                name: "mediation_case_session",
                oldComment: "Среща по дело - медиация");

            migrationBuilder.AlterTable(
                name: "mediation_case_person",
                oldComment: "Страни по делото за срещата");

            migrationBuilder.AlterTable(
                name: "mediation_case_mediator_appraisal_data",
                oldComment: "Таблица с подробни данни за оценка на медиатори към дело");

            migrationBuilder.AlterTable(
                name: "mediation_case_mediator_appraisal",
                oldComment: "Таблица с оценка на медиатори към дело");

            migrationBuilder.AlterTable(
                name: "mediation_case_mediator",
                oldComment: "Таблица с медиатори към дело");

            migrationBuilder.AlterTable(
                name: "common_mediation_mediator_center",
                oldComment: "Центрове към медиатор");

            migrationBuilder.AlterTable(
                name: "common_mediation_mediator",
                oldComment: "Медиатори");

            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator_center",
                oldComment: "Центрове за медиация към кординатор");

            migrationBuilder.AlterTable(
                name: "common_mediation_coordinator",
                oldComment: "Кординатори на центрове за медиация");

            migrationBuilder.AlterTable(
                name: "common_mediation_center_court",
                oldComment: "Кои съдилища се обслужват от даден център");

            migrationBuilder.AlterTable(
                name: "common_mediation_center",
                oldComment: "Центрове за медиация");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_group_id",
                table: "nom_mediation_result_base",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на група");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_group_id",
                table: "nom_mediation_result",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на група");

            migrationBuilder.AlterColumn<bool>(
                name: "without_appraisal",
                table: "nom_mediation_point_mediator_appraisal",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Флаг дали е ред на който не се дава оценка");

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_session_result",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на резултат от среща");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_result_base_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на основание за резултат от среща");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_session_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на среща");

            migrationBuilder.AlterColumn<bool>(
                name: "is_main",
                table: "mediation_case_session_result",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Флаг за основен резултат");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_session_result",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_session_result",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_session_result",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на съд");

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на дело");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_session_result",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_type_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на вид среща: Информационна среща; Процедура по медиация.");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_state_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Статус на срещата");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_location_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на място на което се провежда");

            migrationBuilder.AlterColumn<string>(
                name: "mediation_location_description",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Допълнително пояснение за място на което се провежда");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_session",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "mediation_case_session",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Край: дата и час");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "mediation_case_session",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Начало: дата и час");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_session",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на съд");

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_session",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на дело");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_session",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_person",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_person_session_state_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на статус на лицето след срещата - присъства и т.н.");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_session_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на среща");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_person",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_person",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_person",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на съд");

            migrationBuilder.AlterColumn<int>(
                name: "case_person_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на страна от делото");

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на дело");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_person",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "rating",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Оценка");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_point_mediator_appraisal_id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на точка за оценяване");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_mediator_appraisal_id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на медиатор в дело");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_mediator_appraisal_data",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_mediator_appraisal",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_case_mediator_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на медиатор в дело");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_mediator_appraisal",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_mediator_appraisal",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_mediator_appraisal",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_appraisal",
                table: "mediation_case_mediator_appraisal",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата на оценка");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на съд");

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на дело");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_mediator_appraisal",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "mediation_case_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_mediator_id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на медиатор");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "mediation_case_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "mediation_case_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "mediation_case_mediator",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "До");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "mediation_case_mediator",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "От");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "mediation_case_mediator",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на съд");

            migrationBuilder.AlterColumn<int>(
                name: "case_id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на дело");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "mediation_case_mediator",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_mediator_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_mediator_id",
                table: "common_mediation_mediator_center",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на медиатор");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_centers_id",
                table: "common_mediation_mediator_center",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на център");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_mediator_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_mediator_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_mediator_center",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "До");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_mediator_center",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "От");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_mediator_center",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_mediator_center",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "practice_specific_area_law_year",
                table: "common_mediation_mediator",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Практика в определена област на правото");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Имена на медиатора");

            migrationBuilder.AlterColumn<string>(
                name: "main_profession",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Основна професия");

            migrationBuilder.AlterColumn<string>(
                name: "experience_mediation",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Опит на медиатора в медиация по опредени видове спорове");

            migrationBuilder.AlterColumn<string>(
                name: "education",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Образование");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "До дата");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "От дата");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_entry",
                table: "common_mediation_mediator",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на вписване");

            migrationBuilder.AlterColumn<string>(
                name: "additional_qualification",
                table: "common_mediation_mediator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Допълнителна квалификация");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_mediator",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_coordinator_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_coordinator_id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на кординатор");

            migrationBuilder.AlterColumn<int>(
                name: "mediation_centers_id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на център");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_coordinator_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_coordinator_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_coordinator_center",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "До");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_coordinator_center",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "От");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_coordinator_center",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_coordinator_center",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<string>(
                name: "position",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Длъжност");

            migrationBuilder.AlterColumn<string>(
                name: "lawunit_user_id",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребителя на лицето");

            migrationBuilder.AlterColumn<int>(
                name: "lawunit_id",
                table: "common_mediation_coordinator",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на лице");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_coordinator",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Забележка");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_coordinator",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "До");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_coordinator",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "От");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_coordinator",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_coordinator",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "mediation_center_id",
                table: "common_mediation_center_court",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на център за медиация");

            migrationBuilder.AlterColumn<int>(
                name: "court_id",
                table: "common_mediation_center_court",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на съд");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_center_court",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "user_expired_id",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на потребител, който е анулирал записа");

            migrationBuilder.AlterColumn<int>(
                name: "parent_id",
                table: "common_mediation_center",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на родител");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "common_mediation_center",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Име на центъра");

            migrationBuilder.AlterColumn<string>(
                name: "description_expired",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Причина за анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Описание");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_to",
                table: "common_mediation_center",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "До дата");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_from",
                table: "common_mediation_center",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "От дата");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_expired",
                table: "common_mediation_center",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на анулиране");

            migrationBuilder.AlterColumn<string>(
                name: "contact_details",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Контактни данни");

            migrationBuilder.AlterColumn<string>(
                name: "address_text",
                table: "common_mediation_center",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Адрес");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "common_mediation_center",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на запис")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
