using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_mediation",
                table: "case_h",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_mediation",
                table: "case",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "common_mediation_center",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    address_text = table.Column<string>(type: "text", nullable: true),
                    contact_details = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_mediation_center", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_mediation_center_common_mediation_center_parent_id",
                        column: x => x.parent_id,
                        principalTable: "common_mediation_center",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_center_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "nom_mediation_location",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mediation_location", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_mediation_point_mediator_appraisal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mediation_point_mediator_appraisal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_mediation_result_group",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mediation_result_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_mediation_state",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mediation_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nom_mediation_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mediation_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "common_mediation_center_court",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mediation_center_id = table.Column<int>(type: "integer", nullable: true),
                    court_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_mediation_center_court", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_mediation_center_court_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_mediation_center_court_common_mediation_center_media~",
                        column: x => x.mediation_center_id,
                        principalTable: "common_mediation_center",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "common_mediation_coordinator",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lawunit_id = table.Column<int>(type: "integer", nullable: false),
                    lawunit_user_id = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<string>(type: "text", nullable: true),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    MediationCenterId = table.Column<int>(type: "integer", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_mediation_coordinator", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_common_law_unit_lawunit_id",
                        column: x => x.lawunit_id,
                        principalTable: "common_law_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_common_mediation_center_Mediat~",
                        column: x => x.MediationCenterId,
                        principalTable: "common_mediation_center",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_identity_users_lawunit_user_id",
                        column: x => x.lawunit_user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "common_mediation_mediator",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    additional_qualification = table.Column<string>(type: "text", nullable: true),
                    main_profession = table.Column<string>(type: "text", nullable: true),
                    practice_specific_area_law_year = table.Column<int>(type: "integer", nullable: true),
                    experience_mediation = table.Column<string>(type: "text", nullable: true),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    MediationCenterId = table.Column<int>(type: "integer", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_mediation_mediator", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_mediation_mediator_common_mediation_center_Mediation~",
                        column: x => x.MediationCenterId,
                        principalTable: "common_mediation_center",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_mediator_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_mediator_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "nom_mediation_result",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mediation_result_group_id = table.Column<int>(type: "integer", nullable: true),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mediation_result", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_mediation_result_nom_mediation_result_group_mediation_r~",
                        column: x => x.mediation_result_group_id,
                        principalTable: "nom_mediation_result_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "nom_mediation_result_base",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mediation_result_group_id = table.Column<int>(type: "integer", nullable: true),
                    order_number = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_mediation_result_base", x => x.id);
                    table.ForeignKey(
                        name: "FK_nom_mediation_result_base_nom_mediation_result_group_mediat~",
                        column: x => x.mediation_result_group_id,
                        principalTable: "nom_mediation_result_group",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mediation_case_session",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_type_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_location_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_location_description = table.Column<string>(type: "text", nullable: true),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mediation_state_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_session", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_nom_mediation_location_mediation_loc~",
                        column: x => x.mediation_location_id,
                        principalTable: "nom_mediation_location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_nom_mediation_state_mediation_state_~",
                        column: x => x.mediation_state_id,
                        principalTable: "nom_mediation_state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_nom_mediation_type_mediation_type_id",
                        column: x => x.mediation_type_id,
                        principalTable: "nom_mediation_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_mediation_coordinator_center",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mediation_coordinator_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_centers_id = table.Column<int>(type: "integer", nullable: false),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_mediation_coordinator_center", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_center_common_mediation_center~",
                        column: x => x.mediation_centers_id,
                        principalTable: "common_mediation_center",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_center_common_mediation_coordi~",
                        column: x => x.mediation_coordinator_id,
                        principalTable: "common_mediation_coordinator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_center_identity_users_user_exp~",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_coordinator_center_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "common_mediation_mediator_center",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mediation_mediator_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_centers_id = table.Column<int>(type: "integer", nullable: false),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_mediation_mediator_center", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_mediation_mediator_center_common_mediation_center_me~",
                        column: x => x.mediation_centers_id,
                        principalTable: "common_mediation_center",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_mediation_mediator_center_common_mediation_mediator_~",
                        column: x => x.mediation_mediator_id,
                        principalTable: "common_mediation_mediator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_mediation_mediator_center_identity_users_user_expire~",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_common_mediation_mediator_center_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mediation_case_mediator",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_mediator_id = table.Column<int>(type: "integer", nullable: false),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_mediator", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_common_mediation_mediator_mediation~",
                        column: x => x.mediation_mediator_id,
                        principalTable: "common_mediation_mediator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mediation_case_person",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_case_session_id = table.Column<int>(type: "integer", nullable: false),
                    case_person_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_person_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_person_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_person_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_person_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_person_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_person_mediation_case_session_mediation_case~",
                        column: x => x.mediation_case_session_id,
                        principalTable: "mediation_case_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mediation_case_session_result",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_case_session_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_result_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_result_base_id = table.Column<int>(type: "integer", nullable: true),
                    is_main = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_session_result", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_result_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_result_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_result_identity_users_user_expired_id",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_result_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_result_mediation_case_session_mediat~",
                        column: x => x.mediation_case_session_id,
                        principalTable: "mediation_case_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_session_result_nom_mediation_result_base_med~",
                        column: x => x.mediation_result_base_id,
                        principalTable: "nom_mediation_result_base",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_session_result_nom_mediation_result_mediatio~",
                        column: x => x.mediation_result_id,
                        principalTable: "nom_mediation_result",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mediation_case_mediator_appraisal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    court_id = table.Column<int>(type: "integer", nullable: true),
                    case_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_case_mediator_id = table.Column<int>(type: "integer", nullable: false),
                    date_appraisal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    date_expired = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_expired_id = table.Column<string>(type: "text", nullable: true),
                    description_expired = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    date_wrt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_transfered_dw = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_mediator_appraisal", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_appraisal_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_appraisal_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_appraisal_identity_users_user_expir~",
                        column: x => x.user_expired_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_appraisal_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_appraisal_mediation_case_mediator_m~",
                        column: x => x.mediation_case_mediator_id,
                        principalTable: "mediation_case_mediator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mediation_case_mediator_appraisal_data",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mediation_case_mediator_appraisal_id = table.Column<int>(type: "integer", nullable: false),
                    mediation_point_mediator_appraisal_id = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mediation_case_mediator_appraisal_data", x => x.id);
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_appraisal_data_mediation_case_media~",
                        column: x => x.mediation_case_mediator_appraisal_id,
                        principalTable: "mediation_case_mediator_appraisal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mediation_case_mediator_appraisal_data_nom_mediation_point_~",
                        column: x => x.mediation_point_mediator_appraisal_id,
                        principalTable: "nom_mediation_point_mediator_appraisal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_center_parent_id",
                table: "common_mediation_center",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_center_user_id",
                table: "common_mediation_center",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_center_court_court_id",
                table: "common_mediation_center_court",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_center_court_mediation_center_id",
                table: "common_mediation_center_court",
                column: "mediation_center_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_lawunit_id",
                table: "common_mediation_coordinator",
                column: "lawunit_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_lawunit_user_id",
                table: "common_mediation_coordinator",
                column: "lawunit_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_MediationCenterId",
                table: "common_mediation_coordinator",
                column: "MediationCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_user_expired_id",
                table: "common_mediation_coordinator",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_user_id",
                table: "common_mediation_coordinator",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_center_mediation_centers_id",
                table: "common_mediation_coordinator_center",
                column: "mediation_centers_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_center_mediation_coordinator_id",
                table: "common_mediation_coordinator_center",
                column: "mediation_coordinator_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_center_user_expired_id",
                table: "common_mediation_coordinator_center",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_coordinator_center_user_id",
                table: "common_mediation_coordinator_center",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_MediationCenterId",
                table: "common_mediation_mediator",
                column: "MediationCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_user_expired_id",
                table: "common_mediation_mediator",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_user_id",
                table: "common_mediation_mediator",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_center_mediation_centers_id",
                table: "common_mediation_mediator_center",
                column: "mediation_centers_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_center_mediation_mediator_id",
                table: "common_mediation_mediator_center",
                column: "mediation_mediator_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_center_user_expired_id",
                table: "common_mediation_mediator_center",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_mediation_mediator_center_user_id",
                table: "common_mediation_mediator_center",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_case_id",
                table: "mediation_case_mediator",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_court_id",
                table: "mediation_case_mediator",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_mediation_mediator_id",
                table: "mediation_case_mediator",
                column: "mediation_mediator_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_user_expired_id",
                table: "mediation_case_mediator",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_user_id",
                table: "mediation_case_mediator",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_case_id",
                table: "mediation_case_mediator_appraisal",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_court_id",
                table: "mediation_case_mediator_appraisal",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_mediation_case_mediator_id",
                table: "mediation_case_mediator_appraisal",
                column: "mediation_case_mediator_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_user_expired_id",
                table: "mediation_case_mediator_appraisal",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_user_id",
                table: "mediation_case_mediator_appraisal",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_data_mediation_case_media~",
                table: "mediation_case_mediator_appraisal_data",
                column: "mediation_case_mediator_appraisal_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_mediator_appraisal_data_mediation_point_medi~",
                table: "mediation_case_mediator_appraisal_data",
                column: "mediation_point_mediator_appraisal_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_person_case_id",
                table: "mediation_case_person",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_person_case_person_id",
                table: "mediation_case_person",
                column: "case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_person_court_id",
                table: "mediation_case_person",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_person_mediation_case_session_id",
                table: "mediation_case_person",
                column: "mediation_case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_person_user_expired_id",
                table: "mediation_case_person",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_person_user_id",
                table: "mediation_case_person",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_case_id",
                table: "mediation_case_session",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_court_id",
                table: "mediation_case_session",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_mediation_location_id",
                table: "mediation_case_session",
                column: "mediation_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_mediation_state_id",
                table: "mediation_case_session",
                column: "mediation_state_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_mediation_type_id",
                table: "mediation_case_session",
                column: "mediation_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_user_expired_id",
                table: "mediation_case_session",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_user_id",
                table: "mediation_case_session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_result_case_id",
                table: "mediation_case_session_result",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_result_court_id",
                table: "mediation_case_session_result",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_result_mediation_case_session_id",
                table: "mediation_case_session_result",
                column: "mediation_case_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_result_mediation_result_base_id",
                table: "mediation_case_session_result",
                column: "mediation_result_base_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_result_mediation_result_id",
                table: "mediation_case_session_result",
                column: "mediation_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_result_user_expired_id",
                table: "mediation_case_session_result",
                column: "user_expired_id");

            migrationBuilder.CreateIndex(
                name: "IX_mediation_case_session_result_user_id",
                table: "mediation_case_session_result",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_mediation_result_mediation_result_group_id",
                table: "nom_mediation_result",
                column: "mediation_result_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_nom_mediation_result_base_mediation_result_group_id",
                table: "nom_mediation_result_base",
                column: "mediation_result_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_mediation_center_court");

            migrationBuilder.DropTable(
                name: "common_mediation_coordinator_center");

            migrationBuilder.DropTable(
                name: "common_mediation_mediator_center");

            migrationBuilder.DropTable(
                name: "mediation_case_mediator_appraisal_data");

            migrationBuilder.DropTable(
                name: "mediation_case_person");

            migrationBuilder.DropTable(
                name: "mediation_case_session_result");

            migrationBuilder.DropTable(
                name: "common_mediation_coordinator");

            migrationBuilder.DropTable(
                name: "mediation_case_mediator_appraisal");

            migrationBuilder.DropTable(
                name: "nom_mediation_point_mediator_appraisal");

            migrationBuilder.DropTable(
                name: "mediation_case_session");

            migrationBuilder.DropTable(
                name: "nom_mediation_result_base");

            migrationBuilder.DropTable(
                name: "nom_mediation_result");

            migrationBuilder.DropTable(
                name: "mediation_case_mediator");

            migrationBuilder.DropTable(
                name: "nom_mediation_location");

            migrationBuilder.DropTable(
                name: "nom_mediation_state");

            migrationBuilder.DropTable(
                name: "nom_mediation_type");

            migrationBuilder.DropTable(
                name: "nom_mediation_result_group");

            migrationBuilder.DropTable(
                name: "common_mediation_mediator");

            migrationBuilder.DropTable(
                name: "common_mediation_center");

            migrationBuilder.DropColumn(
                name: "is_mediation",
                table: "case_h");

            migrationBuilder.DropColumn(
                name: "is_mediation",
                table: "case");
        }
    }
}
