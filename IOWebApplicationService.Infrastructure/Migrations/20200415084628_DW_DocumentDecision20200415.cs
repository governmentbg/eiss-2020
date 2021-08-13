using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_DocumentDecision20200415 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_document_decision",
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
                    document_id = table.Column<long>(nullable: false),
                    decision_type_id = table.Column<int>(nullable: true),
                    decision_type_name = table.Column<string>(nullable: true),
                    reg_number = table.Column<string>(nullable: true),
                    reg_date = table.Column<DateTime>(nullable: true),
                    out_document_id = table.Column<long>(nullable: true),
                    user_decision_id = table.Column<string>(nullable: true),
                    user_decision_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    document_decision_state_id = table.Column<int>(nullable: false),
                    document_decision_state_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_document_decision", x => x.dw_Id);
                });

            migrationBuilder.CreateTable(
                name: "dw_document_decision_case",
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
                    document_decision_id = table.Column<long>(nullable: false),
                    case_id = table.Column<int>(nullable: false),
                    decision_type_id = table.Column<int>(nullable: true),
                    decision_type_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_document_decision_case", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_document_decision");

            migrationBuilder.DropTable(
                name: "dw_document_decision_case");
        }
    }
}
