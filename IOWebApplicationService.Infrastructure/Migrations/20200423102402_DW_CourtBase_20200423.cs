using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_CourtBase_20200423 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_case_session_lawunit",
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
                    table.PrimaryKey("PK_dw_case_session_lawunit", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_case_session_lawunit");
        }
    }
}
