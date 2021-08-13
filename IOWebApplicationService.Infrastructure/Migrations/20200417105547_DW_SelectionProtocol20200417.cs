using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_SelectionProtocol20200417 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_case_selection_protocol",
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
                    judge_role_id = table.Column<int>(nullable: false),
                    judge_role_name = table.Column<string>(nullable: true),
                    selection_mode_id = table.Column<int>(nullable: false),
                    selection_mode_name = table.Column<string>(nullable: true),
                    court_duty_id = table.Column<int>(nullable: true),
                    court_duty_name = table.Column<string>(nullable: true),
                    court_group_id = table.Column<int>(nullable: true),
                    court_group_name = table.Column<string>(nullable: true),
                    speciality_id = table.Column<int>(nullable: true),
                    speciality_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    selection_date = table.Column<DateTime>(nullable: false),
                    selected_lawunit_id = table.Column<int>(nullable: true),
                    selected_lawunit_name = table.Column<string>(nullable: true),
                    case_lawunit_dismisal_id = table.Column<int>(nullable: true),
                    selection_protokol_state_id = table.Column<int>(nullable: false),
                    selection_protokol_state_name = table.Column<string>(nullable: true),
                    include_compartment_judges = table.Column<bool>(nullable: false),
                    compartment_id = table.Column<int>(nullable: true),
                    compartment_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_selection_protocol", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_selection_protocol_compartment",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
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
                    court_id = table.Column<int>(nullable: true),
                    case_id = table.Column<int>(nullable: true),
                    case_selection_protokol_id = table.Column<int>(nullable: false),
                    lawunit_id = table.Column<int>(nullable: false),
                    lawunit_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_selection_protocol_compartment", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_case_selection_protocol_lawunit",
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
                    case_id = table.Column<int>(nullable: true),
                    case_selection_protokol_id = table.Column<int>(nullable: false),
                    selected_from_case_group = table.Column<bool>(nullable: false),
                    case_group_id = table.Column<int>(nullable: true),
                    case_group_name = table.Column<string>(nullable: true),
                    lawunit_id = table.Column<int>(nullable: false),
                    lawunit_name = table.Column<string>(nullable: true),
                    load_index = table.Column<int>(nullable: false),
                    case_count = table.Column<int>(nullable: false),
                    state_id = table.Column<int>(nullable: false),
                    state_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_selection_protocol_lawunit", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_case_selection_protocol");

            migrationBuilder.DropTable(
                name: "dw_case_selection_protocol_compartment");

            migrationBuilder.DropTable(
                name: "dw_case_selection_protocol_lawunit");
        }
    }
}
