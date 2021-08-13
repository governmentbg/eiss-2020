using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_CasePerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_case_person",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    dw_count = table.Column<int>(nullable: true),
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
                    user_name = table.Column<string>(nullable: true),
                    id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    case_session_id = table.Column<int>(nullable: true),
                    person_role_id = table.Column<int>(nullable: false),
                    person_role_name = table.Column<string>(nullable: true),
                    military_rang_id = table.Column<int>(nullable: true),
                    military_rang_name = table.Column<string>(nullable: true),
                    person_maturity_id = table.Column<int>(nullable: true),
                    person_maturity_name = table.Column<string>(nullable: true),
                    is_initial_person = table.Column<bool>(nullable: false),
                    case_person_identificator = table.Column<string>(nullable: true),
                    date_from = table.Column<DateTime>(nullable: false),
                    date_from_str = table.Column<string>(nullable: true),
                    date_to = table.Column<DateTime>(nullable: true),
                    date_to_str = table.Column<string>(nullable: true),
                    row_number = table.Column<int>(nullable: false),
                    for_notification = table.Column<bool>(nullable: true),
                    notification_number = table.Column<int>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    is_arrested = table.Column<bool>(nullable: true),
                    birth_country_code = table.Column<string>(nullable: true),
                    birth_country_name = table.Column<string>(nullable: true),
                    birth_city_code = table.Column<string>(nullable: true),
                    birth_foreign_place = table.Column<string>(nullable: true),
                    date_expired = table.Column<DateTime>(nullable: true),
                    date_expired_str = table.Column<string>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    user_expired_name = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true),
                    link_relations_string = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_person", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_case_person");
        }
    }
}
