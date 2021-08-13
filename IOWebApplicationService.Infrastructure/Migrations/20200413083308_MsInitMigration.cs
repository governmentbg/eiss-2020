using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class MsInitMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_case",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    case_id = table.Column<int>(nullable: false),
                    document_id = table.Column<long>(nullable: false),
                    document_name = table.Column<string>(nullable: true),
                    document_type_id = table.Column<long>(nullable: false),
                    document_type_name = table.Column<string>(nullable: true),
                    process_priority_id = table.Column<int>(nullable: true),
                    process_priority_name = table.Column<string>(nullable: true),
                    eispp_number = table.Column<string>(nullable: true),
                    short_number = table.Column<string>(nullable: true),
                    short_number_value = table.Column<int>(nullable: true),
                    reg_number = table.Column<string>(nullable: true),
                    reg_date = table.Column<DateTime>(nullable: false),
                    is_old_number = table.Column<bool>(nullable: true),
                    case_group_id = table.Column<int>(nullable: false),
                    case_group_name = table.Column<string>(nullable: true),
                    case_character_id = table.Column<int>(nullable: false),
                    case_character_name = table.Column<string>(nullable: true),
                    case_type_id = table.Column<int>(nullable: false),
                    case_type_name = table.Column<string>(nullable: true),
                    case_type_code = table.Column<string>(nullable: true),
                    case_code_id = table.Column<int>(nullable: true),
                    case_code_name = table.Column<string>(nullable: true),
                    court_group_id = table.Column<int>(nullable: true),
                    court_group_name = table.Column<string>(nullable: true),
                    load_group_link_id = table.Column<int>(nullable: true),
                    load_group_id = table.Column<int>(nullable: true),
                    load_group_name = table.Column<string>(nullable: true),
                    complex_index = table.Column<decimal>(nullable: false),
                    case_reason_id = table.Column<int>(nullable: true),
                    case_reason_name = table.Column<string>(nullable: true),
                    case_type_unit_id = table.Column<int>(nullable: true),
                    case_type_unit_name = table.Column<string>(nullable: true),
                    load_index = table.Column<decimal>(nullable: false),
                    correction_load_index = table.Column<decimal>(nullable: true),
                    is_resticted_access = table.Column<bool>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    case_state_id = table.Column<int>(nullable: false),
                    case_state_name = table.Column<string>(nullable: true),
                    case_duration_months = table.Column<int>(nullable: false),
                    case_inforced_date = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_lawunit",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    case_session_id = table.Column<int>(nullable: true),
                    lawunit_id = table.Column<int>(nullable: false),
                    lawunit_full_name = table.Column<string>(nullable: true),
                    judge_role_id = table.Column<int>(nullable: false),
                    judge_role_name = table.Column<string>(nullable: true),
                    court_department_id = table.Column<int>(nullable: true),
                    court_department_name = table.Column<string>(nullable: true),
                    court_duty_id = table.Column<int>(nullable: true),
                    court_duty_name = table.Column<string>(nullable: true),
                    court_group_id = table.Column<int>(nullable: true),
                    court_group_name = table.Column<string>(nullable: true),
                    judge_department_role_id = table.Column<int>(nullable: true),
                    judge_department_role_name = table.Column<string>(nullable: true),
                    date_from = table.Column<DateTime>(nullable: false),
                    date_to = table.Column<DateTime>(nullable: true),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_lawunit", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_session",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    session_type_id = table.Column<int>(nullable: false),
                    session_type_name = table.Column<string>(nullable: true),
                    compartment_id = table.Column<int>(nullable: false),
                    compartment_name = table.Column<string>(nullable: true),
                    court_hall_id = table.Column<int>(nullable: true),
                    court_hall_name = table.Column<string>(nullable: true),
                    date_from = table.Column<DateTime>(nullable: false),
                    date_to = table.Column<DateTime>(nullable: true),
                    session_state_id = table.Column<int>(nullable: false),
                    session_state_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    user_expired_name = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true),
                    date_returned = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_session_act",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    case_session_id = table.Column<int>(nullable: false),
                    act_type_id = table.Column<int>(nullable: false),
                    act_type_name = table.Column<string>(nullable: true),
                    act_kind_id = table.Column<int>(nullable: true),
                    act_kind_name = table.Column<string>(nullable: true),
                    act_result_id = table.Column<int>(nullable: true),
                    act_result_name = table.Column<string>(nullable: true),
                    reg_number = table.Column<string>(nullable: true),
                    reg_date = table.Column<DateTime>(nullable: true),
                    reg_number_full = table.Column<string>(nullable: true),
                    is_final_doc = table.Column<bool>(nullable: false),
                    is_ready_for_publish = table.Column<bool>(nullable: false),
                    act_date = table.Column<DateTime>(nullable: true),
                    act_declared_date = table.Column<DateTime>(nullable: true),
                    act_motives_declared_date = table.Column<DateTime>(nullable: true),
                    act_inforced_date = table.Column<DateTime>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    act_state_id = table.Column<int>(nullable: false),
                    act_state_name = table.Column<string>(nullable: true),
                    secretary_user_id = table.Column<string>(nullable: true),
                    secretary_user_name = table.Column<string>(nullable: true),
                    depersonalize_user_id = table.Column<string>(nullable: true),
                    depersonalize_user_name = table.Column<string>(nullable: true),
                    can_appeal = table.Column<bool>(nullable: true),
                    act_complain_result_id = table.Column<int>(nullable: true),
                    act_complain_result_state_id = table.Column<int>(nullable: true),
                    act_complain_result_state_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session_act", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_session_act_complain",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    case_session_act_id = table.Column<int>(nullable: false),
                    complain_document_id = table.Column<long>(nullable: false),
                    document_name = table.Column<string>(nullable: true),
                    document_type_id = table.Column<long>(nullable: false),
                    document_type_name = table.Column<string>(nullable: true),
                    reject_description = table.Column<string>(nullable: true),
                    complaint_state_id = table.Column<int>(nullable: false),
                    complaint_state_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session_act_complain", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_session_act_complain_person",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    case_session_act_complain_id = table.Column<int>(nullable: false),
                    complain_case_person_id = table.Column<int>(nullable: false),
                    complain_case_person_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session_act_complain_person", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_session_act_complain_result",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    case_session_act_complain_id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    case_short_number_value = table.Column<int>(nullable: true),
                    case_reg_number = table.Column<string>(nullable: true),
                    case_reg_date = table.Column<DateTime>(nullable: false),
                    case_session_act_id = table.Column<int>(nullable: false),
                    act_result_id = table.Column<int>(nullable: true),
                    act_result_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    date_result = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session_act_complain_result", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_session_act_coordination",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    case_session_act_id = table.Column<int>(nullable: false),
                    case_lawunit_id = table.Column<int>(nullable: false),
                    case_lawunit_name = table.Column<string>(nullable: true),
                    act_coordination_type_id = table.Column<int>(nullable: false),
                    act_coordination_type_name = table.Column<string>(nullable: true),
                    content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session_act_coordination", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_session_act_divorce",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session_act_divorce", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_document",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false),
                    document_direction_id = table.Column<int>(nullable: false),
                    document_direction_name = table.Column<string>(nullable: true),
                    document_group_id = table.Column<int>(nullable: false),
                    document_group_name = table.Column<string>(nullable: true),
                    document_type_id = table.Column<int>(nullable: false),
                    document_type_name = table.Column<string>(nullable: true),
                    document_number_value = table.Column<int>(nullable: true),
                    document_number = table.Column<string>(nullable: true),
                    document_date = table.Column<DateTime>(nullable: false),
                    actual_document_date = table.Column<DateTime>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    is_resticted_access = table.Column<bool>(nullable: false),
                    is_secret = table.Column<bool>(nullable: true),
                    is_old_number = table.Column<bool>(nullable: true),
                    delivery_group_id = table.Column<int>(nullable: true),
                    delivery_group_name = table.Column<string>(nullable: true),
                    delivery_type_id = table.Column<int>(nullable: true),
                    delivery_type_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_document", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_case");

            migrationBuilder.DropTable(
                name: "dw_case_lawunit");

            migrationBuilder.DropTable(
                name: "dw_case_session");

            migrationBuilder.DropTable(
                name: "dw_case_session_act");

            migrationBuilder.DropTable(
                name: "dw_case_session_act_complain");

            migrationBuilder.DropTable(
                name: "dw_case_session_act_complain_person");

            migrationBuilder.DropTable(
                name: "dw_case_session_act_complain_result");

            migrationBuilder.DropTable(
                name: "dw_case_session_act_coordination");

            migrationBuilder.DropTable(
                name: "dw_case_session_act_divorce");

            migrationBuilder.DropTable(
                name: "dw_document");
        }
    }
}
