using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_CaseSessionResult20200414_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_case_session_result",
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
                    case_session_id = table.Column<int>(nullable: false),
                    session_result_id = table.Column<int>(nullable: false),
                    session_result_name = table.Column<string>(nullable: true),
                    session_result_base_id = table.Column<int>(nullable: true),
                    session_result_base_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    is_main = table.Column<bool>(nullable: false),
                    date_expired = table.Column<DateTime>(nullable: true),
                    user_expired_id = table.Column<string>(nullable: true),
                    user_expired_name = table.Column<string>(nullable: true),
                    description_expired = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_case_session_result", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_case_session_result");
        }
    }
}
